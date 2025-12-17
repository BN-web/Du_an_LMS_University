using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMS_GV.Models.Data;
using BCrypt.Net;
using LMS_GV.Models;

namespace LMS_GV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                // Tìm người dùng theo tên đăng nhập
                var nguoiDung = await _context.NguoiDungs
                    .Include(u => u.VaiTro)
                    .FirstOrDefaultAsync(u => u.TenDangNhap == request.Username);

                if (nguoiDung == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Tên đăng nhập hoặc mật khẩu không đúng"
                    });
                }

                // Kiểm tra mật khẩu
                if (nguoiDung.HashMatKhau == null || !BCrypt.Net.BCrypt.Verify(request.Password, nguoiDung.HashMatKhau))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Tên đăng nhập hoặc mật khẩu không đúng"
                    });
                }

                // Kiểm tra trạng thái tài khoản
                if (nguoiDung.TrangThai != 1) // 1 = đang hoạt động
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Tài khoản của bạn đã bị khóa hoặc chưa kích hoạt"
                    });
                }

                // Tạo token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, nguoiDung.NguoiDungId.ToString()),
                        new Claim(ClaimTypes.Name, nguoiDung.TenDangNhap),
                        new Claim(ClaimTypes.Email, nguoiDung.Email ?? ""),
                        new Claim(ClaimTypes.Role, nguoiDung.VaiTroId?.ToString() ?? "0"),
                        new Claim("FullName", nguoiDung.HoTen ?? ""),
                        new Claim("Avatar", nguoiDung.Avatar ?? "")
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new
                {
                    success = true,
                    token = tokenString,
                    user = new
                    {
                        id = nguoiDung.NguoiDungId,
                        username = nguoiDung.TenDangNhap,
                        email = nguoiDung.Email,
                        fullName = nguoiDung.HoTen,
                        avatar = nguoiDung.Avatar,
                        roleId = nguoiDung.VaiTroId,
                        roleName = nguoiDung.VaiTro?.TenVaiTro
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                // Kiểm tra username đã tồn tại chưa
                var existingUser = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.TenDangNhap == request.Username);

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên đăng nhập đã tồn tại"
                    });
                }

                // Kiểm tra email đã tồn tại chưa
                if (!string.IsNullOrEmpty(request.Email))
                {
                    existingUser = await _context.NguoiDungs
                        .FirstOrDefaultAsync(u => u.Email == request.Email);

                    if (existingUser != null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Email đã được sử dụng"
                        });
                    }
                }

                // Mã hóa mật khẩu
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Tạo người dùng mới
                var newUser = new NguoiDung
                {
                    TenDangNhap = request.Username,
                    HashMatKhau = hashedPassword,
                    Email = request.Email,
                    HoTen = request.FullName,
                    NgaySinh = request.DateOfBirth,
                    GioiTinh = request.Gender,
                    DiaChi = request.Address,
                    SoDienThoai = request.PhoneNumber,
                    TrangThai = 1, // Đang hoạt động
                    VaiTroId = 4 // Mặc định là sinh viên
                };

                _context.NguoiDungs.Add(newUser);
                await _context.SaveChangesAsync();

                // Tạo hồ sơ sinh viên
                var sinhVien = new HoSoSinhVien
                {
                    NguoiDungId = newUser.NguoiDungId,
                    Mssv = GenerateMSSV(), // Hàm tạo MSSV tự động
                    ThoiGianDaoTao = "2024-2028",
                    TongTinChi = 0,
                    Gpa = 0,
                    NganhId = request.MajorId, // Cần truyền từ request
                    KhoaTuyenSinhId = request.CourseId // Cần truyền từ request
                };

                _context.HoSoSinhViens.Add(sinhVien);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Đăng ký thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        private string GenerateMSSV()
        {
            // Logic tạo MSSV tự động
            var year = DateTime.Now.Year;
            var random = new Random();
            return $"{year}{random.Next(1000, 9999)}";
        }
    }

    public class LoginRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int? MajorId { get; set; }
        public int? CourseId { get; set; }
    }
}