using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using LMS_GV.DTOs;
using BCrypt.Net;

namespace LMS_GV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthController> _logger;

        public GoogleAuthController(AppDbContext context, IConfiguration configuration, ILogger<GoogleAuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập / đăng ký tài khoản bằng Google.
        /// Nếu lần đầu login sẽ tạo NguoiDung + HoSoSinhVien (nếu là sinh viên) hoặc GiangVien (nếu là trưởng khoa).
        /// Nếu đã tồn tại sẽ cập nhật thông tin và trả về JWT.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<GoogleLoginResponseDTO>> LoginWithGoogle([FromBody] GoogleLoginRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.GoogleId) || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new GoogleLoginResponseDTO
                    {
                        Success = false,
                        Message = "GoogleId và Email là bắt buộc"
                    });
                }

                // 1. Tìm NguoiDung theo email hoặc GoogleId
                var nguoiDung = await _context.NguoiDungs
                    .Include(u => u.VaiTro)
                    .FirstOrDefaultAsync(u => 
                        u.Email == request.Email || 
                        u.DangnhapGoogle == request.GoogleId);

                int? vaiTroId = null;
                string roleName = "Sinh viên";

                if (nguoiDung == null)
                {
                    // 2.1. Tạo NguoiDung mới
                    // Tạo tên đăng nhập từ email (phần trước @)
                    var tenDangNhap = request.Email.Split('@')[0];
                    var tenDangNhapGoc = tenDangNhap;
                    int counter = 1;

                    // Đảm bảo tên đăng nhập unique
                    while (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == tenDangNhap))
                    {
                        tenDangNhap = $"{tenDangNhapGoc}{counter}";
                        counter++;
                    }

                    // Tạo mật khẩu ngẫu nhiên (không dùng cho login Google nhưng cần có)
                    var randomPassword = Guid.NewGuid().ToString("N");
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword);

                    // Mặc định là Sinh viên (VaiTroId = 4)
                    vaiTroId = 4;
                    roleName = "Sinh viên";

                    // Normalize string để tránh lỗi encoding
                    var normalizedName = NormalizeString(request.Name ?? request.Email);

                    nguoiDung = new NguoiDung
                    {
                        TenDangNhap = tenDangNhap,
                        HashMatKhau = passwordHash,
                        Email = request.Email,
                        HoTen = normalizedName,
                        DangnhapGoogle = request.GoogleId,
                        EmailGoogleVerified = true,
                        Avatar = !string.IsNullOrWhiteSpace(request.PictureUrl) ? request.PictureUrl : null,
                        TrangThai = 1, // Đang hoạt động
                        VaiTroId = vaiTroId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        LanDangNhapCuoi = DateTime.Now
                    };

                    _context.NguoiDungs.Add(nguoiDung);
                    await _context.SaveChangesAsync();

                    // Tạo hồ sơ Sinh viên
                    var sinhVien = new HoSoSinhVien
                    {
                        NguoiDungId = nguoiDung.NguoiDungId,
                        Mssv = GenerateMSSV(),
                        ThoiGianDaoTao = $"{DateTime.Now.Year}-{DateTime.Now.Year + 4}",
                        TongTinChi = 0,
                        Gpa = null,
                        CreatedAt = DateTime.Now
                    };

                    _context.HoSoSinhViens.Add(sinhVien);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // 2.2. Cập nhật thông tin người dùng đã tồn tại
                    nguoiDung.DangnhapGoogle = request.GoogleId;
                    nguoiDung.EmailGoogleVerified = true;
                    nguoiDung.LanDangNhapCuoi = DateTime.Now;
                    nguoiDung.UpdatedAt = DateTime.Now;

                    // Cập nhật thông tin nếu có
                    if (!string.IsNullOrWhiteSpace(request.Name))
                    {
                        // Normalize string để tránh lỗi encoding
                        nguoiDung.HoTen = NormalizeString(request.Name);
                    }

                    if (!string.IsNullOrWhiteSpace(request.PictureUrl))
                    {
                        nguoiDung.Avatar = request.PictureUrl;
                    }

                    // Xác định vai trò
                    vaiTroId = nguoiDung.VaiTroId;
                    roleName = nguoiDung.VaiTro?.TenVaiTro ?? "Sinh viên";

                    // Kiểm tra xem có hồ sơ Sinh viên hoặc Giảng viên chưa
                    var sinhVien = await _context.HoSoSinhViens
                        .FirstOrDefaultAsync(sv => sv.NguoiDungId == nguoiDung.NguoiDungId);

                    var giangVien = await _context.GiangViens
                        .FirstOrDefaultAsync(gv => gv.NguoiDungId == nguoiDung.NguoiDungId);

                    // Nếu chưa có hồ sơ nào, tạo hồ sơ Sinh viên mặc định
                    if (sinhVien == null && giangVien == null)
                    {
                        // Nếu vai trò là Sinh viên, tạo hồ sơ sinh viên
                        if (vaiTroId == 4)
                        {
                            sinhVien = new HoSoSinhVien
                            {
                                NguoiDungId = nguoiDung.NguoiDungId,
                                Mssv = GenerateMSSV(),
                                ThoiGianDaoTao = $"{DateTime.Now.Year}-{DateTime.Now.Year + 4}",
                                TongTinChi = 0,
                                Gpa = null,
                                CreatedAt = DateTime.Now
                            };
                            _context.HoSoSinhViens.Add(sinhVien);
                        }
                    }

                    _context.NguoiDungs.Update(nguoiDung);
                    await _context.SaveChangesAsync();
                }

                // 3. Lấy thông tin đầy đủ để trả về
                var sinhVienInfo = await _context.HoSoSinhViens
                    .FirstOrDefaultAsync(sv => sv.NguoiDungId == nguoiDung.NguoiDungId);

                var giangVienInfo = await _context.GiangViens
                    .FirstOrDefaultAsync(gv => gv.NguoiDungId == nguoiDung.NguoiDungId);

                // 4. Sinh JWT token
                var token = GenerateJwtToken(nguoiDung, vaiTroId?.ToString() ?? "4");

                var userInfo = new GoogleUserInfoDTO
                {
                    NguoiDungId = nguoiDung.NguoiDungId,
                    Email = nguoiDung.Email ?? "",
                    HoTen = nguoiDung.HoTen,
                    Avatar = nguoiDung.Avatar,
                    VaiTroId = vaiTroId,
                    TenVaiTro = roleName,
                    SinhVienId = sinhVienInfo?.SinhVienId,
                    GiangVienId = giangVienInfo?.GiangVienId,
                    MSSV = sinhVienInfo?.Mssv
                };

                var response = new GoogleLoginResponseDTO
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    User = userInfo
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập bằng Google");
                return StatusCode(500, new GoogleLoginResponseDTO
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Đăng nhập cho Trưởng khoa bằng Google
        /// CHỈ cho phép đăng nhập với user có NguoiDungId = 4 (truongkhoa1@gmail.com)
        /// KHÔNG tạo user mới
        /// </summary>
        [HttpPost("login/truong-khoa")]
        public async Task<ActionResult<GoogleLoginResponseDTO>> LoginWithGoogleTruongKhoa([FromBody] GoogleLoginRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.GoogleId) || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new GoogleLoginResponseDTO
                    {
                        Success = false,
                        Message = "GoogleId và Email là bắt buộc"
                    });
                }

                // CHỈ cho phép đăng nhập với user có NguoiDungId = 4
                const int ALLOWED_USER_ID = 4;

                // 1. Tìm user với ID = 4
                var nguoiDung = await _context.NguoiDungs
                    .Include(u => u.VaiTro)
                    .FirstOrDefaultAsync(u => u.NguoiDungId == ALLOWED_USER_ID);

                // Nếu không tìm thấy user với ID = 4, từ chối đăng nhập
                if (nguoiDung == null)
                {
                    return Unauthorized(new GoogleLoginResponseDTO
                    {
                        Success = false,
                        Message = "Tài khoản không được phép đăng nhập bằng Google"
                    });
                }

                // Kiểm tra VaiTroId phải là 3 (Trưởng khoa)
                if (nguoiDung.VaiTroId != 3)
                {
                    return Unauthorized(new GoogleLoginResponseDTO
                    {
                        Success = false,
                        Message = "Tài khoản này không phải là Trưởng khoa"
                    });
                }

                // Cập nhật GoogleId nếu chưa có hoặc khác
                if (string.IsNullOrWhiteSpace(nguoiDung.DangnhapGoogle) || nguoiDung.DangnhapGoogle != request.GoogleId)
                {
                    nguoiDung.DangnhapGoogle = request.GoogleId;
                }

                int vaiTroId = 3; // Trưởng khoa
                string roleName = nguoiDung.VaiTro?.TenVaiTro ?? "Trưởng khoa";

                // KHÔNG tạo user mới - chỉ cập nhật thông tin user hiện có (user ID = 4)
                nguoiDung.EmailGoogleVerified = true;
                nguoiDung.LanDangNhapCuoi = DateTime.Now;
                nguoiDung.UpdatedAt = DateTime.Now;

                // Cập nhật tên từ Google nếu có
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    nguoiDung.HoTen = NormalizeString(request.Name);
                }

                // Cập nhật Avatar từ Google picture
                if (!string.IsNullOrWhiteSpace(request.PictureUrl))
                {
                    nguoiDung.Avatar = request.PictureUrl;
                }

                // Đảm bảo vai trò là Trưởng khoa
                if (nguoiDung.VaiTroId != 3)
                {
                    nguoiDung.VaiTroId = 3;
                }
                
                // Reload VaiTro để lấy TenVaiTro
                await _context.Entry(nguoiDung).Reference(u => u.VaiTro).LoadAsync();
                if (nguoiDung.VaiTro != null)
                {
                    roleName = nguoiDung.VaiTro.TenVaiTro;
                }

                // Đảm bảo có hồ sơ Giảng viên
                var giangVien = await _context.GiangViens
                    .FirstOrDefaultAsync(gv => gv.NguoiDungId == nguoiDung.NguoiDungId);

                if (giangVien == null)
                {
                    giangVien = new GiangVien
                    {
                        NguoiDungId = nguoiDung.NguoiDungId,
                        ChucVu = "Trưởng khoa",
                        KhoaId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.GiangViens.Add(giangVien);
                }
                else
                {
                    // Đảm bảo chức vụ là Trưởng khoa
                    if (giangVien.ChucVu != "Trưởng khoa")
                    {
                        giangVien.ChucVu = "Trưởng khoa";
                        giangVien.UpdatedAt = DateTime.Now;
                        _context.GiangViens.Update(giangVien);
                    }
                }

                _context.NguoiDungs.Update(nguoiDung);
                await _context.SaveChangesAsync();

                // 3. Lấy thông tin đầy đủ
                var giangVienInfo = await _context.GiangViens
                    .FirstOrDefaultAsync(gv => gv.NguoiDungId == nguoiDung.NguoiDungId);

                // 4. Sinh JWT token - dùng roleName "Trưởng khoa" thay vì roleId "3" để match với [Authorize(Roles = "Trưởng khoa")]
                var token = GenerateJwtToken(nguoiDung, roleName);

                var userInfo = new GoogleUserInfoDTO
                {
                    NguoiDungId = nguoiDung.NguoiDungId,
                    Email = nguoiDung.Email ?? "",
                    HoTen = nguoiDung.HoTen,
                    Avatar = nguoiDung.Avatar,
                    VaiTroId = vaiTroId,
                    TenVaiTro = roleName,
                    GiangVienId = giangVienInfo?.GiangVienId
                };

                var response = new GoogleLoginResponseDTO
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    User = userInfo
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập Trưởng khoa bằng Google");
                return StatusCode(500, new GoogleLoginResponseDTO
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo JWT token
        /// </summary>
        private string GenerateJwtToken(NguoiDung nguoiDung, string roleId)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new Exception("JWT Key không được cấu hình");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "LMS_GV_API";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "LMS_GV_Client";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.NguoiDungId.ToString()),
                new Claim(ClaimTypes.Name, nguoiDung.TenDangNhap),
                new Claim(ClaimTypes.Email, nguoiDung.Email ?? ""),
                new Claim(ClaimTypes.Role, roleId),
                new Claim("FullName", nguoiDung.HoTen ?? ""),
                new Claim("Avatar", nguoiDung.Avatar ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Tạo MSSV tự động
        /// </summary>
        private string GenerateMSSV()
        {
            var year = DateTime.Now.Year;
            var random = new Random();
            var randomPart = random.Next(100000, 999999);
            return $"{year}{randomPart}";
        }

        /// <summary>
        /// Normalize string để tránh lỗi encoding khi lưu vào database
        /// Xử lý trường hợp double-encoding (ví dụ: "nguyễn" -> "nguyá»nn")
        /// </summary>
        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            try
            {
                // Kiểm tra xem có phải là double-encoded không
                // Double-encoded thường có pattern như "á»", "á¼", etc.
                if (input.Contains("á") && (input.Contains("»") || input.Contains("¼") || input.Contains("©")))
                {
                    // Có thể đã bị double-encode, thử decode lại
                    // Chuyển string thành bytes với encoding Latin-1 (ISO-8859-1)
                    // rồi decode lại như UTF-8
                    byte[] latin1Bytes = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(input);
                    string decoded = System.Text.Encoding.UTF8.GetString(latin1Bytes);
                    
                    // Kiểm tra xem kết quả có hợp lý không (không còn các ký tự lạ)
                    if (!decoded.Contains("á") || !decoded.Contains("»"))
                    {
                        return decoded;
                    }
                }
                
                // Nếu không phải double-encode, trả về nguyên bản
                return input;
            }
            catch
            {
                // Nếu có lỗi, trả về nguyên bản
                return input;
            }
        }
    }
}
