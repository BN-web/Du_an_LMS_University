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
            var token = _jwt.GenerateGiangVienToken(user);

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


        [HttpPost("google-login")]
        public async Task<ActionResult<GoogleUserResponseDTO>> LoginWithGoogle(
            [FromBody] GoogleLoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Tìm user ĐÃ TỒN TẠI
            var user = await _db.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u =>
                    u.DangnhapGoogle == request.GoogleId ||
                    u.Email == request.Email
                );

            // ❌ KHÔNG TỒN TẠI → CẤM ĐĂNG NHẬP
            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Tài khoản Google chưa được hệ thống cấp quyền"
                });
            }

            // ❌ TÀI KHOẢN BỊ KHÓA
            if (user.TrangThai != 1)
            {
                return Unauthorized(new
                {
                    message = "Tài khoản đã bị khóa hoặc ngừng hoạt động"
                });
            }

            // 2. Cập nhật thông tin Google (KHÔNG TẠO MỚI)
            user.DangnhapGoogle = request.GoogleId;
            user.EmailGoogleVerified = request.EmailVerified ?? user.EmailGoogleVerified;
            user.Avatar = request.PictureUrl ?? user.Avatar;
            user.UpdatedAt = DateTime.UtcNow;
            user.LanDangNhapCuoi = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // 3. Sinh JWT
            var token = _jwt.GenerateGiangVienToken(user);

            // 4. Trả kết quả
            return Ok(new GoogleUserResponseDTO
            {
                NguoiDungId = user.NguoiDungId,
                Email = user.Email ?? "",
                Name = user.HoTen ?? "",
                Avatar = user.Avatar,
                Role = "Giảng Viên",
                Token = token
            });

        }
    }
}
