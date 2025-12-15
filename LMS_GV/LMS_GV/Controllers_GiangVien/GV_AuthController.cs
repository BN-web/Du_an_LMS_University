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
        public async Task<ActionResult<GoogleUserResponseDTO>> LoginWithGoogle([FromBody] GoogleLoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.GoogleId) || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "GoogleId và Email là bắt buộc" });

            // 1. Tìm người dùng theo GoogleId hoặc Email
            var user = await _db.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.DangnhapGoogle == request.GoogleId || u.Email == request.Email);

            string roleName = "Giảng Viên";

            // Nếu chưa có → tạo mới
            if (user == null)
            {
                // Lấy VaiTroId của giảng viên
                var giangVienRole = await _db.VaiTros.FirstOrDefaultAsync(r => r.TenVaiTro == "Giảng Viên");
                int roleId = giangVienRole?.VaiTroId ?? 2;

                user = new NguoiDung
                {
                    TenDangNhap = request.Email,
                    Email = request.Email,
                    HoTen = request.Name ?? request.Email,
                    Avatar = request.PictureUrl,
                    EmailGoogleVerified = request.EmailVerified ?? true,
                    DangnhapGoogle = request.GoogleId,
                    TrangThai = 1,
                    VaiTroId = roleId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.NguoiDungs.Add(user);
            }
            else
            {
                // Nếu đã tồn tại → cập nhật thông tin
                user.HoTen = request.Name ?? user.HoTen;
                user.Avatar = request.PictureUrl ?? user.Avatar;
                user.EmailGoogleVerified = request.EmailVerified ?? user.EmailGoogleVerified;
                user.DangnhapGoogle = request.GoogleId;
                user.UpdatedAt = DateTime.UtcNow;

                roleName = user.VaiTro?.TenVaiTro ?? "Giảng Viên";
            }

            await _db.SaveChangesAsync();

            // 3. Sinh JWT token
            var token = GenerateJwtToken(user, roleName);

            // 4. Trả về DTO
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
