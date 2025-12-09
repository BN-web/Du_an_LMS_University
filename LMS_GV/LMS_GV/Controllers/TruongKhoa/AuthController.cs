using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace LMS_GV.Controllers.TruongKhoa
{
    [ApiController]
    [Route("api/truong-khoa/auth")]
    public class TruongKhoaAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public TruongKhoaAuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public class TruongKhoaLoginRequest
        {
            [Required]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] TruongKhoaLoginRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var identifier = req.Email?.Trim();
            var user = await _db.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.Email == identifier || u.TenDangNhap == identifier);

            if (user == null || string.IsNullOrEmpty(user.HashMatKhau))
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(req.Password.Trim(), (user.HashMatKhau ?? string.Empty).Trim());
            if (!isPasswordValid)
                return Unauthorized(new { message = "Sai mật khẩu" });

            var roleId = user.VaiTroId ?? 0;
            var roleName = user.VaiTro?.TenVaiTro ?? string.Empty;

            if (!(roleId == 3 || roleName.Equals("TruongMon", StringComparison.OrdinalIgnoreCase)))
                return Unauthorized(new { message = "Không có quyền Trưởng môn" });

            user.LanDangNhapCuoi = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.NguoiDungId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, string.IsNullOrEmpty(user.TenDangNhap) ? (user.Email ?? user.NguoiDungId.ToString()) : user.TenDangNhap),
                new Claim(ClaimTypes.Role, "TruongMon")
            };

            var accessToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            var accessTokenStr = new JwtSecurityTokenHandler().WriteToken(accessToken);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                accessToken = accessTokenStr
            });
        }
    }
}
