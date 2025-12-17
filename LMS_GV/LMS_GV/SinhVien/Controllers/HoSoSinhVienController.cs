using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LMS_GV.SinhVien.DTOs;
using LMS_GV.Models.Data;
using System.IO;
using System.Globalization;
using System.Net.Http;
using System.Net.Http;

namespace LMS_GV.SinhVien.Controllers
{
    [Route("api/sinhvien")]
    [ApiController]
    public class HoSoSinhVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<HoSoSinhVienController> _logger;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly IHttpClientFactory _httpClientFactory;

        public HoSoSinhVienController(
            AppDbContext context,
            IWebHostEnvironment environment,
            ILogger<HoSoSinhVienController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // Helper method để lấy thông tin người dùng từ token
        private int? GetCurrentNguoiDungId()
        {
            var nguoiDungIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nguoiDungIdClaim))
                return null;

            if (int.TryParse(nguoiDungIdClaim, out int nguoiDungId))
                return nguoiDungId;

            return null;
        }

        // 1. GET: api/sinhvien/hoso - Lấy thông tin hồ sơ sinh viên
        [HttpGet("hoso")]
        public async Task<IActionResult> GetHoSoSinhVien()
        {
            try
            {
                var nguoiDungId = GetCurrentNguoiDungId();
                if (!nguoiDungId.HasValue)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc không có quyền truy cập"
                    });
                }

                // Lấy thông tin hồ sơ sinh viên
                var hoSoSinhVien = await _context.HoSoSinhViens
                    .Include(h => h.NguoiDung)
                    .Include(h => h.Nganh)
                        .ThenInclude(n => n.Khoa)
                    .Include(h => h.KhoaTuyenSinh)
                    .FirstOrDefaultAsync(h => h.NguoiDungId == nguoiDungId);

                if (hoSoSinhVien == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy hồ sơ sinh viên"
                    });
                }

                // Lấy danh sách lớp học của sinh viên
                var sinhVienLopList = await _context.SinhVienLops
                    .Where(svl => svl.SinhVienId == hoSoSinhVien.SinhVienId)
                    .Include(svl => svl.LopHoc)
                        .ThenInclude(lh => lh.MonHoc)
                    .Include(svl => svl.LopHoc)
                        .ThenInclude(lh => lh.Nganh)
                    .Include(svl => svl.LopHoc)
                        .ThenInclude(lh => lh.GiangVien)
                        .ThenInclude(gv => gv.NguoiDung)
                    .Include(svl => svl.LopHoc)
                        .ThenInclude(lh => lh.HocKy)
                    .ToListAsync();

                // Lấy các lớp đang học (trạng thái lớp = 1)
                var lopDangHoc = sinhVienLopList
                    .Where(svl => svl.LopHoc != null && svl.LopHoc.TrangThai == 1)
                    .Select(svl => svl.LopHoc)
                    .Distinct()
                    .ToList();

                // Lấy các lớp đã hoàn thành (có điểm)
                var lopDaHoanThanh = await _context.Diems
                    .Where(d => d.SinhVienId == hoSoSinhVien.SinhVienId && d.LopHoc != null)
                    .Include(d => d.LopHoc)
                        .ThenInclude(lh => lh.MonHoc)
                    .Include(d => d.LopHoc)
                        .ThenInclude(lh => lh.Nganh)
                    .Include(d => d.LopHoc)
                        .ThenInclude(lh => lh.GiangVien)
                        .ThenInclude(gv => gv.NguoiDung)
                    .Include(d => d.LopHoc)
                        .ThenInclude(lh => lh.HocKy)
                    .Select(d => d.LopHoc)
                    .Distinct()
                    .ToListAsync();

                // Format ngày sinh
                string? ngaySinhFormatted = null;
                if (hoSoSinhVien.NguoiDung?.NgaySinh.HasValue == true)
                {
                    ngaySinhFormatted = hoSoSinhVien.NguoiDung.NgaySinh.Value.ToString("dd/MM/yyyy");
                }

                // Chuẩn hóa URL avatar
                string? avatarUrl = null;
                if (!string.IsNullOrEmpty(hoSoSinhVien.NguoiDung?.Avatar))
                {
                    if (hoSoSinhVien.NguoiDung.Avatar.StartsWith("http"))
                    {
                        avatarUrl = hoSoSinhVien.NguoiDung.Avatar;
                    }
                    else if (hoSoSinhVien.NguoiDung.Avatar.StartsWith("/"))
                    {
                        // Nếu avatar bắt đầu bằng / thì giữ nguyên
                        avatarUrl = hoSoSinhVien.NguoiDung.Avatar;
                    }
                    else
                    {
                        // Thêm / vào đầu nếu cần
                        avatarUrl = "/" + hoSoSinhVien.NguoiDung.Avatar;
                    }
                }

                // Tạo DTO response
                var response = new HoSoSinhVienResponseDTO
                {
                    ThongTinCaNhan = new ThongTinCaNhanDTO
                    {
                        AnhDaiDien = avatarUrl,
                        HoTen = hoSoSinhVien.NguoiDung?.HoTen ?? "Chưa cập nhật",
                        MaSv = hoSoSinhVien.Mssv ?? "Chưa có MSSV",
                        NgaySinh = ngaySinhFormatted,
                        GioiTinh = hoSoSinhVien.NguoiDung?.GioiTinh,
                        Email = hoSoSinhVien.NguoiDung?.Email,
                        DiaChi = hoSoSinhVien.NguoiDung?.DiaChi,
                        DiemGPA = hoSoSinhVien.Gpa,
                        TongTinChi = hoSoSinhVien.TongTinChi,
                        Khoa = hoSoSinhVien.Nganh?.Khoa?.TenKhoa,
                        Nganh = hoSoSinhVien.Nganh?.TenNganh,
                        ThoiGianDaoTao = hoSoSinhVien.ThoiGianDaoTao
                    },
                    DanhSachMonDangHoc = lopDangHoc
                        .Where(lh => lh != null && lh.MonHoc != null)
                        .Select(lh => new MonDangHocDTO
                        {
                            MaMon = lh.MonHoc?.MaMon,
                            TenMon = lh.MonHoc?.TenMon ?? "Chưa có tên môn",
                            Nganh = lh.Nganh?.TenNganh,
                            TinChi = lh.SoTinChi ?? lh.MonHoc?.SoTinChi,
                            GiangVien = lh.GiangVien?.NguoiDung?.HoTen,
                            EmailGiangVien = lh.GiangVien?.NguoiDung?.Email
                        })
                        .ToList(),
                    LichSuMonDaHoanThanh = lopDaHoanThanh
                        .Where(lh => lh != null && lh.MonHoc != null)
                        .Select(lh => new MonDaHoanThanhDTO
                        {
                            MaMon = lh.MonHoc?.MaMon,
                            TenMon = lh.MonHoc?.TenMon ?? "Chưa có tên môn",
                            Nganh = lh.Nganh?.TenNganh,
                            TinChi = lh.SoTinChi ?? lh.MonHoc?.SoTinChi,
                            ThoiGian = FormatThoiGian(lh.NgayBatDau, lh.NgayKetThuc),
                            GiangVien = lh.GiangVien?.NguoiDung?.HoTen,
                            EmailGiangVien = lh.GiangVien?.NguoiDung?.Email
                        })
                        .ToList()
                };

                return Ok(new ApiResponseDTO<HoSoSinhVienResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hồ sơ sinh viên");
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // Helper method để format thời gian
        private string FormatThoiGian(DateOnly? ngayBatDau, DateOnly? ngayKetThuc)
        {
            if (!ngayBatDau.HasValue && !ngayKetThuc.HasValue)
                return "Chưa xác định";

            if (ngayBatDau.HasValue && ngayKetThuc.HasValue)
            {
                return $"{ngayBatDau.Value:dd/MM/yyyy}-{ngayKetThuc.Value:dd/MM/yyyy}";
            }
            else if (ngayBatDau.HasValue)
            {
                return $"Từ {ngayBatDau.Value:dd/MM/yyyy}";
            }
            else
            {
                return $"Đến {ngayKetThuc.Value:dd/MM/yyyy}";
            }
        }

        // 2. PUT: api/sinhvien/avatar - Cập nhật avatar bằng URL hoặc path từ uploads
        [HttpPut("avatar")]
        public async Task<IActionResult> CapNhatAvatar([FromBody] CapNhatAvatarRequestDTO request)
        {
            try
            {
                var nguoiDungId = GetCurrentNguoiDungId();
                if (!nguoiDungId.HasValue)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc không có quyền truy cập"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.AvatarUrl))
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "URL avatar không được để trống"
                    });
                }

                // Tìm người dùng
                var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId.Value);
                if (nguoiDung == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Chuẩn hóa URL avatar
                string avatarUrl = request.AvatarUrl.Trim();

                // Nếu là URL external (http/https), giữ nguyên
                if (avatarUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    avatarUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // URL external - giữ nguyên
                }
                // Nếu là path từ uploads (bắt đầu bằng /uploads/avatars/)
                else if (avatarUrl.StartsWith("/uploads/avatars/", StringComparison.OrdinalIgnoreCase))
                {
                    // Kiểm tra file có tồn tại không
                    var fileName = avatarUrl.Replace("/uploads/avatars/", "");
                    var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "avatars");
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Nếu không có trong wwwroot, thử tìm trong uploads root
                    if (!System.IO.File.Exists(filePath))
                    {
                        var uploadsRootFolder = Path.Combine(_environment.ContentRootPath, "uploads", "avatars");
                        filePath = Path.Combine(uploadsRootFolder, fileName);
                    }

                    if (!System.IO.File.Exists(filePath))
                    {
                        return BadRequest(new ApiResponseDTO<object>
                        {
                            Success = false,
                            Message = $"File ảnh không tồn tại: {avatarUrl}"
                        });
                    }

                    // Đảm bảo path bắt đầu bằng /
                    if (!avatarUrl.StartsWith("/"))
                    {
                        avatarUrl = "/" + avatarUrl;
                    }
                }
                // Nếu là path tương đối (chỉ có tên file hoặc path ngắn)
                else if (avatarUrl.Contains("/") || avatarUrl.Contains("\\"))
                {
                    // Nếu không bắt đầu bằng /, thêm vào
                    if (!avatarUrl.StartsWith("/"))
                    {
                        avatarUrl = "/" + avatarUrl;
                    }
                }
                // Nếu chỉ có tên file (không có path)
                else
                {
                    // Giả định file nằm trong uploads/avatars
                    avatarUrl = $"/uploads/avatars/{avatarUrl}";
                    
                    // Kiểm tra file có tồn tại không
                    var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "avatars");
                    var filePath = Path.Combine(uploadsFolder, avatarUrl.Replace("/uploads/avatars/", ""));

                    if (!System.IO.File.Exists(filePath))
                    {
                        var uploadsRootFolder = Path.Combine(_environment.ContentRootPath, "uploads", "avatars");
                        filePath = Path.Combine(uploadsRootFolder, avatarUrl.Replace("/uploads/avatars/", ""));
                    }

                    if (!System.IO.File.Exists(filePath))
                    {
                        return BadRequest(new ApiResponseDTO<object>
                        {
                            Success = false,
                            Message = $"File ảnh không tồn tại: {avatarUrl}"
                        });
                    }
                }

                // Xóa avatar cũ nếu có (chỉ xóa nếu là file local)
                if (!string.IsNullOrEmpty(nguoiDung.Avatar) && !nguoiDung.Avatar.StartsWith("http"))
                {
                    try
                    {
                        var oldAvatarPath = nguoiDung.Avatar;
                        if (oldAvatarPath.StartsWith("/"))
                        {
                            oldAvatarPath = oldAvatarPath.Substring(1);
                        }

                        var oldFilePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", oldAvatarPath);
                        if (!System.IO.File.Exists(oldFilePath))
                        {
                            oldFilePath = Path.Combine(_environment.ContentRootPath, oldAvatarPath);
                        }

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể xóa avatar cũ: {Avatar}", nguoiDung.Avatar);
                    }
                }

                // Cập nhật avatar
                nguoiDung.Avatar = avatarUrl;
                nguoiDung.UpdatedAt = DateTime.Now;

                _context.NguoiDungs.Update(nguoiDung);
                await _context.SaveChangesAsync();

                var response = new CapNhatAvatarResponseDTO
                {
                    AvatarUrl = avatarUrl,
                    ThongBao = "Cập nhật avatar thành công"
                };

                return Ok(new ApiResponseDTO<CapNhatAvatarResponseDTO>
                {
                    Success = true,
                    Message = "Cập nhật avatar thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật avatar");
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 3. POST: api/sinhvien/avatar/upload - Upload file avatar
        // 3. POST: api/sinhvien/avatar/upload - Upload file avatar
        [HttpPost("avatar/upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequestDTO request)
        {
            try
            {
                var nguoiDungId = GetCurrentNguoiDungId();
                if (!nguoiDungId.HasValue)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc không có quyền truy cập"
                    });
                }

                var file = request.File;

                // Kiểm tra file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Vui lòng chọn file ảnh"
                    });
                }

                // Kiểm tra kích thước file
                if (file.Length > MaxFileSize)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = $"Kích thước file quá lớn. Tối đa {MaxFileSize / 1024 / 1024}MB"
                    });
                }

                // Kiểm tra định dạng file
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = $"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedExtensions)}"
                    });
                }

                // Tìm người dùng
                var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId.Value);
                if (nguoiDung == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Tạo thư mục uploads/avatars theo cấu trúc ảnh
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất
                var fileName = $"avatar_{nguoiDungId}_{DateTime.Now:yyyyMMddHHmmssfff}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Tạo URL để truy cập file (theo cấu trúc uploads/avatars/)
                var fileUrl = $"/uploads/avatars/{fileName}";

                // Xóa avatar cũ nếu có (chỉ xóa nếu là file local)
                if (!string.IsNullOrEmpty(nguoiDung.Avatar) && !nguoiDung.Avatar.StartsWith("http"))
                {
                    try
                    {
                        var oldAvatarPath = nguoiDung.Avatar;
                        if (oldAvatarPath.StartsWith("/"))
                        {
                            oldAvatarPath = oldAvatarPath.Substring(1);
                        }

                        var oldFilePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", oldAvatarPath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể xóa avatar cũ: {Avatar}", nguoiDung.Avatar);
                    }
                }

                // Cập nhật avatar mới
                nguoiDung.Avatar = fileUrl;
                nguoiDung.UpdatedAt = DateTime.Now;

                _context.NguoiDungs.Update(nguoiDung);
                await _context.SaveChangesAsync();

                var response = new UploadAvatarResponseDTO
                {
                    FileName = fileName,
                    FileUrl = fileUrl,
                    FileSize = file.Length,
                    ThongBao = "Upload avatar thành công"
                };

                return Ok(new ApiResponseDTO<UploadAvatarResponseDTO>
                {
                    Success = true,
                    Message = "Upload avatar thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload avatar");
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 4. DELETE: api/sinhvien/avatar - Xóa avatar
        [HttpDelete("avatar")]
        public async Task<IActionResult> XoaAvatar()
        {
            try
            {
                var nguoiDungId = GetCurrentNguoiDungId();
                if (!nguoiDungId.HasValue)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc không có quyền truy cập"
                    });
                }

                // Tìm người dùng
                var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId.Value);
                if (nguoiDung == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Xóa file avatar cũ nếu có (chỉ xóa nếu là file local)
                if (!string.IsNullOrEmpty(nguoiDung.Avatar) && !nguoiDung.Avatar.StartsWith("http"))
                {
                    try
                    {
                        var oldAvatarPath = nguoiDung.Avatar;
                        if (oldAvatarPath.StartsWith("/"))
                        {
                            oldAvatarPath = oldAvatarPath.Substring(1);
                        }

                        var oldFilePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", oldAvatarPath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể xóa file avatar cũ: {Avatar}", nguoiDung.Avatar);
                    }
                }

                // Xóa avatar trong database
                nguoiDung.Avatar = null;
                nguoiDung.UpdatedAt = DateTime.Now;

                _context.NguoiDungs.Update(nguoiDung);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Message = "Xóa avatar thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa avatar");
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 5. PUT: api/sinhvien/cap-nhat-thong-tin - Cập nhật thông tin cá nhân
        [HttpPut("cap-nhat-thong-tin")]
        public async Task<IActionResult> CapNhatThongTinCaNhan([FromBody] ThongTinCaNhanDTO request)
        {
            try
            {
                var nguoiDungId = GetCurrentNguoiDungId();
                if (!nguoiDungId.HasValue)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc không có quyền truy cập"
                    });
                }

                // Tìm người dùng
                var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId.Value);
                if (nguoiDung == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Cập nhật thông tin
                if (!string.IsNullOrWhiteSpace(request.HoTen))
                    nguoiDung.HoTen = request.HoTen;

                if (!string.IsNullOrWhiteSpace(request.Email))
                    nguoiDung.Email = request.Email;

                if (!string.IsNullOrWhiteSpace(request.DiaChi))
                    nguoiDung.DiaChi = request.DiaChi;

                if (!string.IsNullOrWhiteSpace(request.GioiTinh))
                    nguoiDung.GioiTinh = request.GioiTinh;

                if (!string.IsNullOrWhiteSpace(request.NgaySinh))
                {
                    if (DateTime.TryParseExact(request.NgaySinh, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngaySinh))
                    {
                        nguoiDung.NgaySinh = ngaySinh;
                    }
                }

                nguoiDung.UpdatedAt = DateTime.Now;

                _context.NguoiDungs.Update(nguoiDung);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Message = "Cập nhật thông tin thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin cá nhân");
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 6. GET: api/sinhvien/avatar - Lấy URL avatar hiện tại
        [HttpGet("avatar")]
        public async Task<IActionResult> GetAvatar()
        {
            try
            {
                var nguoiDungId = GetCurrentNguoiDungId();
                if (!nguoiDungId.HasValue)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc không có quyền truy cập"
                    });
                }

                // Tìm người dùng
                var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId.Value);
                if (nguoiDung == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Data = new { AvatarUrl = nguoiDung.Avatar }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy avatar");
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 7. GET: api/sinhvien/avatar/list - Lấy danh sách ảnh có sẵn trong uploads/avatars
        [HttpGet("avatar/list")]
        public IActionResult GetDanhSachAnh()
        {
            try
            {
                var nguoiDungId = GetCurrentNguoiDungId();
                if (!nguoiDungId.HasValue)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc không có quyền truy cập"
                    });
                }

                var danhSachAnh = new List<AnhItemDTO>();

                // Tìm trong wwwroot/uploads/avatars
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "avatars");
                if (Directory.Exists(uploadsFolder))
                {
                    var files = Directory.GetFiles(uploadsFolder)
                        .Where(f => AllowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.LastWriteTime)
                        .ToList();

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file.FullName);
                        danhSachAnh.Add(new AnhItemDTO
                        {
                            FileName = fileName,
                            FileUrl = $"/uploads/avatars/{fileName}",
                            FileSize = file.Length,
                            LastModified = file.LastWriteTime
                        });
                    }
                }

                // Tìm trong uploads/avatars (root)
                var uploadsRootFolder = Path.Combine(_environment.ContentRootPath, "uploads", "avatars");
                if (Directory.Exists(uploadsRootFolder))
                {
                    var files = Directory.GetFiles(uploadsRootFolder)
                        .Where(f => AllowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.LastWriteTime)
                        .ToList();

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file.FullName);
                        // Chỉ thêm nếu chưa có trong danh sách (tránh trùng lặp)
                        if (!danhSachAnh.Any(a => a.FileName == fileName))
                        {
                            danhSachAnh.Add(new AnhItemDTO
                            {
                                FileName = fileName,
                                FileUrl = $"/uploads/avatars/{fileName}",
                                FileSize = file.Length,
                                LastModified = file.LastWriteTime
                            });
                        }
                    }
                }

                var response = new DanhSachAnhResponseDTO
                {
                    DanhSachAnh = danhSachAnh.OrderByDescending(a => a.LastModified).ToList()
                };

                return Ok(new ApiResponseDTO<DanhSachAnhResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách ảnh");
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 8. POST: api/sinhvien/avatar/download - Tải ảnh từ URL và lưu vào uploads
        [HttpPost("avatar/download")]
        public async Task<IActionResult> TaiAnhTuUrl([FromBody] TaiAnhTuUrlRequestDTO request)
        {
            try
            {
                var nguoiDungId = GetCurrentNguoiDungId();
                if (!nguoiDungId.HasValue)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc không có quyền truy cập"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.ImageUrl))
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "URL ảnh không được để trống"
                    });
                }

                // Kiểm tra URL có hợp lệ không
                if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out Uri? uri) || 
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "URL không hợp lệ. Vui lòng sử dụng URL http hoặc https"
                    });
                }

                // Tìm người dùng
                var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId.Value);
                if (nguoiDung == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng"
                    });
                }

                // Tải ảnh từ URL
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.GetAsync(request.ImageUrl);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Lỗi khi tải ảnh từ URL: {Url}", request.ImageUrl);
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = $"Không thể tải ảnh từ URL: {ex.Message}"
                    });
                }
                catch (TaskCanceledException)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Timeout khi tải ảnh. URL có thể không phản hồi hoặc quá chậm"
                    });
                }

                // Kiểm tra Content-Type
                var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
                if (contentType == null || !contentType.StartsWith("image/"))
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "URL không phải là ảnh hợp lệ"
                    });
                }

                // Đọc dữ liệu ảnh
                byte[] imageData;
                try
                {
                    imageData = await response.Content.ReadAsByteArrayAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi đọc dữ liệu ảnh từ URL: {Url}", request.ImageUrl);
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = $"Không thể đọc dữ liệu ảnh: {ex.Message}"
                    });
                }

                // Kiểm tra kích thước file
                if (imageData.Length > MaxFileSize)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = $"Kích thước ảnh quá lớn. Tối đa {MaxFileSize / 1024 / 1024}MB"
                    });
                }

                // Xác định extension từ URL hoặc Content-Type
                string fileExtension = ".jpg";
                var urlExtension = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
                if (!string.IsNullOrEmpty(urlExtension) && AllowedExtensions.Contains(urlExtension))
                {
                    fileExtension = urlExtension;
                }
                else
                {
                    if (contentType.Contains("jpeg") || contentType.Contains("jpg"))
                        fileExtension = ".jpg";
                    else if (contentType.Contains("png"))
                        fileExtension = ".png";
                    else if (contentType.Contains("gif"))
                        fileExtension = ".gif";
                    else
                    {
                        return BadRequest(new ApiResponseDTO<object>
                        {
                            Success = false,
                            Message = $"Định dạng ảnh không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", AllowedExtensions)}"
                        });
                    }
                }

                // Tạo thư mục uploads/avatars
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất
                var fileName = $"avatar_{nguoiDungId}_{DateTime.Now:yyyyMMddHHmmssfff}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                await System.IO.File.WriteAllBytesAsync(filePath, imageData);

                // Tạo URL để truy cập file
                var fileUrl = $"/uploads/avatars/{fileName}";

                // Xóa avatar cũ nếu có
                if (!string.IsNullOrEmpty(nguoiDung.Avatar) && !nguoiDung.Avatar.StartsWith("http"))
                {
                    try
                    {
                        var oldAvatarPath = nguoiDung.Avatar;
                        if (oldAvatarPath.StartsWith("/"))
                        {
                            oldAvatarPath = oldAvatarPath.Substring(1);
                        }

                        var oldFilePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", oldAvatarPath);
                        if (!System.IO.File.Exists(oldFilePath))
                        {
                            oldFilePath = Path.Combine(_environment.ContentRootPath, oldAvatarPath);
                        }

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể xóa avatar cũ: {Avatar}", nguoiDung.Avatar);
                    }
                }

                // Cập nhật avatar mới
                nguoiDung.Avatar = fileUrl;
                nguoiDung.UpdatedAt = DateTime.Now;

                _context.NguoiDungs.Update(nguoiDung);
                await _context.SaveChangesAsync();

                var result = new TaiAnhTuUrlResponseDTO
                {
                    FileName = fileName,
                    FileUrl = fileUrl,
                    FileSize = imageData.Length,
                    ThongBao = "Tải và lưu ảnh thành công"
                };

                return Ok(new ApiResponseDTO<TaiAnhTuUrlResponseDTO>
                {
                    Success = true,
                    Message = "Tải và lưu ảnh thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải ảnh từ URL");
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}