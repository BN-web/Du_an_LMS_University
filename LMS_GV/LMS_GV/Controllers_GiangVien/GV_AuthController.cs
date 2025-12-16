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
using OtpNet;
using System.Collections.Concurrent;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LMS_GV.Controllers_GiangVien
{
    [ApiController]
    [Route("api/auth")]
    public class GV_AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly IEmailService _emailService;

        // Lưu OTP tạm thời
        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expire)> _otpStore = new();

        public GV_AuthController(AppDbContext db, JwtService jwt, IEmailService emailService)
        {
            _db = db;
            _jwt = jwt;
            _emailService = emailService;
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


        // 1️⃣ Google login → gửi OTP
        [HttpPost("google-login-send-otp")]
        public async Task<IActionResult> GoogleLoginSendOtp([FromBody] GoogleLoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Tìm user theo GoogleId hoặc Email
            var user = await _db.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u =>
                    u.DangnhapGoogle == request.GoogleId || u.Email == request.Email);

            if (user == null)
                return Unauthorized(new { message = "Email chưa được cấp quyền" });

            if (user.TrangThai != 1)
                return Unauthorized(new { message = "Tài khoản bị khóa" });

            if (user.VaiTroId != 2) // chỉ giảng viên
                return Unauthorized(new { message = "Chỉ giảng viên mới được phép đăng nhập" });

            // Cập nhật thông tin Google
            user.DangnhapGoogle = request.GoogleId;
            user.EmailGoogleVerified = request.EmailVerified ?? user.EmailGoogleVerified;
            user.Avatar = request.PictureUrl ?? user.Avatar;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Sinh OTP 6 chữ số
            var otp = new Random().Next(100000, 999999).ToString();
            _otpStore[user.Email] = (otp, DateTime.UtcNow.AddMinutes(5));

            // Gửi OTP tới email
            await _emailService.SendOtpAsync(user.Email, otp);

            return Ok(new SendOtpResponseDTO
            {
                Message = $"OTP đã được gửi đến {user.Email}",
                Success = true
            });
        }

        // 2️⃣ Xác nhận OTP → cấp JWT
        [HttpPost("verify-google-otp")]
        public async Task<IActionResult> VerifyGoogleOtp([FromBody] VerifyOtpRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_otpStore.TryGetValue(request.Email, out var data))
            {
                if (DateTime.UtcNow > data.Expire)
                    return Unauthorized(new { message = "OTP đã hết hạn" });

                if (data.Otp != request.Otp)
                    return Unauthorized(new { message = "OTP không đúng" });

                // OTP hợp lệ → xóa OTP
                _otpStore.TryRemove(request.Email, out _);

                // Lấy user để cấp token
                var user = await _db.NguoiDungs
                    .Include(u => u.VaiTro)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                    return Unauthorized(new { message = "Email không tồn tại" });

                // Sinh JWT
                var token = _jwt.GenerateGiangVienToken(user);

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
                        loginProvider = "google"
                    }
                });
            }

            return NotFound(new { message = "OTP không tồn tại hoặc đã hết hạn" });
        }

    }
}
