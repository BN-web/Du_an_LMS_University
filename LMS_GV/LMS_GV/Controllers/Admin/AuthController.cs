using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LMS_GV.Models;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public class LoginRequest
        {
            [Required(ErrorMessage = "Email là bắt buộc")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
            public string Password { get; set; } = string.Empty;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            // Nếu dùng [ApiController] thì có thể check luôn ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Tìm user theo email hoặc tên đăng nhập
            var identifier = req.Email?.Trim();
            var user = await _db.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.Email == identifier || u.TenDangNhap == identifier);

            // 2. Nếu không tìm thấy user -> giữ message chung
            if (user == null)
            {
                return Unauthorized(new
                {
                    field = "email",
                    message = "Email hoặc mật khẩu không đúng"
                });
            }

            // 3. Nếu user chưa có hash mật khẩu (tuỳ bạn muốn xử lý sao)
            if (string.IsNullOrEmpty(user.HashMatKhau))
            {
                return Unauthorized(new
                {
                    field = "password",
                    message = "Tài khoản chưa được thiết lập mật khẩu"
                });
            }

            // 4. Kiểm tra mật khẩu
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(req.Password.Trim(), (user.HashMatKhau ?? string.Empty).Trim());

            if (!isPasswordValid)
            {
                return Unauthorized(new
                {
                    field = "password",
                    message = "Sai mật khẩu"
                });
            }

            // 4.1. Chỉ cho phép Admin đăng nhập
            if ((user.VaiTroId ?? 0) != 1)
            {
                return Unauthorized(new
                {
                    field = "role",
                    message = "Không có quyền"
                });
            }

            // 5. Đăng nhập thành công -> cập nhật lần đăng nhập cuối
            user.LanDangNhapCuoi = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // 6. Tạo JWT
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.NguoiDungId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var accessToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            var accessTokenStr = new JwtSecurityTokenHandler().WriteToken(accessToken);
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            return Ok(new
            {
                message = "Đăng nhập thành công",
                accessToken = accessTokenStr,
                refreshToken
            });
        }
    }
}