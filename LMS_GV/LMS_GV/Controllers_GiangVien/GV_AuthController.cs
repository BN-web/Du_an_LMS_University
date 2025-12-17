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
using System.Collections.Concurrent;
using System.Net.Sockets;


namespace LMS_GV.Controllers_GiangVien
{
    [ApiController]
    [Route("api/auth")]
    public class GV_AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;



        public GV_AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
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
        [HttpPost("login-google")]
        public async Task<ActionResult<GoogleLoginResponseDTO>> LoginWithGoogle([FromBody] GoogleLoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.GoogleId) || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new GoogleLoginResponseDTO { Success = false, Message = "GoogleId và Email là bắt buộc" });

            // Kiểm tra user đã tồn tại
            var user = await _db.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.DangnhapGoogle == request.GoogleId);

            int roleId = 2; // Giảng viên
            string roleName = "Giảng Viên";

            if (user == null)
            {
                // Tạo user mới
                var randomPassword = Guid.NewGuid().ToString("N");
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword);

                user = new NguoiDung
                {
                    TenDangNhap = request.Email.Split('@')[0],
                    HashMatKhau = passwordHash,
                    Email = request.Email,
                    HoTen = request.Name,
                    DangnhapGoogle = request.GoogleId,
                    Avatar = request.PictureUrl,
                    TrangThai = 1,
                    VaiTroId = roleId,
                    EmailGoogleVerified = request.EmailVerified ?? true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    LanDangNhapCuoi = DateTime.Now
                };

                _db.NguoiDungs.Add(user);
                await _db.SaveChangesAsync();

                // Tạo hồ sơ Giảng Viên
                var giangVien = new GiangVien
                {
                    NguoiDungId = user.NguoiDungId,
                    ChucVu = "Giảng Viên",
                    CreatedAt = DateTime.Now
                };
                _db.GiangViens.Add(giangVien);
                await _db.SaveChangesAsync();
            }
            else
            {
                // Cập nhật Google info
                user.DangnhapGoogle = request.GoogleId;
                user.EmailGoogleVerified = request.EmailVerified ?? true;
                user.HoTen = request.Name ?? user.HoTen;
                user.Avatar = request.PictureUrl ?? user.Avatar;
                user.LanDangNhapCuoi = DateTime.Now;
                _db.NguoiDungs.Update(user);
                await _db.SaveChangesAsync();
            }

            // Tạo token JWT
            var token = _jwt.GenerateGiangVienToken(user);

            var response = new GoogleLoginResponseDTO
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                User = new GoogleUserInfoDTO
                {
                    NguoiDungId = user.NguoiDungId,
                    Email = user.Email,
                    HoTen = user.HoTen,
                    Avatar = user.Avatar,
                    VaiTroId = user.VaiTroId,
                    TenVaiTro = roleName
                }
            };

            return Ok(response);
        }

    }
}
