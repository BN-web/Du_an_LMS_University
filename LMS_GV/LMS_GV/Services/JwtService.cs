using LMS_GV.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(NguoiDung user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.NguoiDungId.ToString()),
            new Claim(ClaimTypes.Role, user.VaiTro?.TenVaiTro ?? "Giảng Viên"),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim("GiangVien_id", (user.VaiTroId == 2 ? user.NguoiDungId.ToString() : ""))
        };

        // Dựa vào DangnhapGoogle để biết login Google hay local
        if (!string.IsNullOrEmpty(user.DangnhapGoogle))
        {
            claims.Add(new Claim("login_provider", "google"));
        }
        else
        {
            claims.Add(new Claim("login_provider", "local"));
        }

        // Kiểm tra JWT Key
        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString))
            throw new Exception("JWT Key is not configured!");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(6),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
