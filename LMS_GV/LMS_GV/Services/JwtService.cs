using LMS_GV.Models.Data;
using LMS_GV.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Data.Entity;
public class JwtService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public JwtService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public string GenerateGiangVienToken(NguoiDung user)
    {
        var giangVienId = _db.GiangViens
            .Where(gv => gv.NguoiDungId == user.NguoiDungId)
            .Select(gv => gv.GiangVienId)
            .FirstOrDefault();

        if (giangVienId == 0)
            throw new Exception("User không có hồ sơ giảng viên");

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.NguoiDungId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(ClaimTypes.Role, "Giảng Viên"),
        new Claim("GiangVien_id", giangVienId.ToString()),
        new Claim("FullName", user.HoTen ?? ""),
        new Claim("Avatar", user.Avatar ?? ""),
        new Claim("VaiTroId", user.VaiTroId.ToString()),
        new Claim("Username", user.TenDangNhap ?? "")
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // đồng bộ với Google login trước
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
