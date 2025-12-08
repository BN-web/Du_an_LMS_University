using Google.Apis.Auth;
using LMS_GV.Models;
using LMS_GV.Models.Data;
using LMS_GV.Models.DTO_GiangVien;
using LMS_GV.Models.DTOs_GiangVien;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMS_GV.Controllers_GiangVien
{
    [ApiController]
    [Route("api/auth")]
    public class GV_AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly IConfiguration _config;

        public GV_AuthController(AppDbContext db, JwtService jwt, IConfiguration config)
        {
            _db = db;
            _jwt = jwt;
            _config = config;
        }


        //Tạo nhanh hash Password
        [HttpPost("create-hash")]
        public IActionResult CreateHash([FromBody] string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return BadRequest(new { message = "Password không được để trống" });

            var hash = BCrypt.Net.BCrypt.HashPassword(password.Trim());
            return Ok(new { hash });
        }


        //Đăng nhập Email + Password
        [HttpPost("login")]
        public IActionResult Login(LoginDTO req)
        {
            // Lấy user theo email
            var user = _db.NguoiDungs.SingleOrDefault(x => x.Email == req.Email);

            if (user == null)
                return BadRequest(new { message = "Sai tài khoản" });

            // Kiểm tra VaiTro: chỉ cho phép Giảng viên
            if (user.VaiTroId != 2)
                return Unauthorized(new { message = "Chỉ giảng viên mới được phép đăng nhập" });

            // Kiểm tra hash bắt đầu bằng $2a (chỉ chấp nhận BCrypt)
            if (string.IsNullOrEmpty(user.HashMatKhau) || !user.HashMatKhau.StartsWith("$2a"))
                return BadRequest(new { message = "Hash mật khẩu không hợp lệ" });

            // Kiểm tra mật khẩu
            bool isValidPassword = false;
            try
            {
                isValidPassword = BCrypt.Net.BCrypt.Verify(req.MatKhau, user.HashMatKhau);
            }
            catch
            {
                return BadRequest(new { message = "Sai mật khẩu" });
            }

            if (!isValidPassword)
                return BadRequest(new { message = "Sai mật khẩu" });

            // Tạo token JWT với claims sub, email, VaiTroId
            var token = _jwt.GenerateToken(user);

            // Cập nhật thời gian đăng nhập cuối
            user.LanDangNhapCuoi = DateTime.Now;
            _db.SaveChanges();

            // Trả thông tin user và token
            return Ok(new
            {
                token,
                user = new
                {
                    user.NguoiDungId,
                    user.Email,
                    user.HoTen,
                    user.VaiTroId,
                    isInstructor = true,
                    loginProvider = "local"
                }
            });
        }

        // Đăng nhập bằng Google 
        [HttpPost("google-login")]
    public async Task<ActionResult<GoogleUserResponseDTO>> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest(new { message = "IdToken của Google là bắt buộc" });

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
            // Có thể kiểm tra Audience để đảm bảo token là cho client của bạn
        }
        catch
        {
            return BadRequest(new { message = "Token Google không hợp lệ" });
        }

        // 1. Tìm hoặc tạo NguoiDung
        var user = await _db.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(u => u.DangnhapGoogle == payload.Subject || u.Email == payload.Email);

        string roleName = "Giảng viên";

        if (user == null)
        {
            // Tạo mới
            user = new NguoiDung
            {
                TenDangNhap = payload.Email,
                Email = payload.Email,
                HoTen = payload.Name ?? payload.Email,
                Avatar = payload.Picture,
                DangnhapGoogle = payload.Subject,
                EmailGoogleVerified = payload.EmailVerified,
                TrangThai = 1,
                CreatedAt = DateTime.UtcNow
            };

            // Gán VaiTro mặc định là Giảng viên
            var instructorRole = await _db.VaiTros.FirstOrDefaultAsync(v => v.TenVaiTro.ToLower() == "giảng viên");
            user.VaiTroId = instructorRole?.VaiTroId ?? 2;

            _db.NguoiDungs.Add(user);
        }
        else
        {
            // Cập nhật thông tin
            user.HoTen = payload.Name ?? user.HoTen;
            user.Avatar = payload.Picture ?? user.Avatar;
            user.DangnhapGoogle = payload.Subject;
            user.EmailGoogleVerified = payload.EmailVerified;
            user.UpdatedAt = DateTime.UtcNow;

            roleName = user.VaiTro?.TenVaiTro ?? "Giảng viên";

            _db.NguoiDungs.Update(user);
        }

        await _db.SaveChangesAsync();

        // 2. Sinh JWT cho ứng dụng
        var token = GenerateJwtToken(user, roleName);

        return Ok(new GoogleUserResponseDTO
        {
            NguoiDungId = user.NguoiDungId,
            Email = user.Email ?? string.Empty,
            Name = user.HoTen ?? string.Empty,
            Avatar = user.Avatar,
            Role = roleName,
            Token = token
        });
    }

    private string GenerateJwtToken(NguoiDung user, string role)
    {
        var jwtKey = _config["Jwt:Key"] ?? "SuperSecretKey_ChangeMe";
        var jwtIssuer = _config["Jwt:Issuer"] ?? "LMS_API";
        var jwtAudience = _config["Jwt:Audience"] ?? "LMS_User";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.NguoiDungId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim("name", user.HoTen ?? string.Empty),
        new Claim(ClaimTypes.Role, role)
    };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
}
