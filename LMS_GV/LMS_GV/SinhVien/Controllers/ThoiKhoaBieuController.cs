using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LMS_GV.SinhVien.DTOs;
using LMS_GV.Models.Data;
using System.Globalization;
using LMS_GV.Models;

namespace LMS_GV.SinhVien.Controllers
{
    [Route("api/sinhvien/thoi-khoa-bieu")]
    [ApiController]
    public class ThoiKhoaBieuController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CultureInfo _cultureInfo = new CultureInfo("vi-VN");
        private readonly ILogger<ThoiKhoaBieuController> _logger;

        public ThoiKhoaBieuController(AppDbContext context, ILogger<ThoiKhoaBieuController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper: Lấy SinhVienId từ token
        private async Task<int?> GetSinhVienId()
        {
            var nguoiDungIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nguoiDungIdClaim) || !int.TryParse(nguoiDungIdClaim, out int nguoiDungId))
            {
                return null;
            }
            
            var sinhVien = await _context.HoSoSinhViens
                .FirstOrDefaultAsync(h => h.NguoiDungId == nguoiDungId);
            return sinhVien?.SinhVienId;
        }

        // Helper: Kiểm tra và trả về error message nếu không phải sinh viên
        private async Task<IActionResult?> CheckSinhVienAccess()
        {
            var nguoiDungIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nguoiDungIdClaim) || !int.TryParse(nguoiDungIdClaim, out int nguoiDungId))
            {
                return Unauthorized(new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "Không tìm thấy thông tin người dùng trong token"
                });
            }

            var nguoiDung = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.NguoiDungId == nguoiDungId);

            if (nguoiDung == null)
            {
                return Unauthorized(new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "Không tìm thấy thông tin người dùng"
                });
            }

            // Kiểm tra vai trò - chỉ cho phép sinh viên (VaiTroId = 4)
            if (nguoiDung.VaiTroId != 4)
            {
                return Unauthorized(new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Bạn không có quyền truy cập API này. API này chỉ dành cho sinh viên. Vai trò hiện tại của bạn: {nguoiDung.VaiTro?.TenVaiTro ?? "Không xác định"}. Vui lòng đăng nhập bằng tài khoản sinh viên."
                });
            }

            var sinhVien = await _context.HoSoSinhViens
                .FirstOrDefaultAsync(h => h.NguoiDungId == nguoiDungId);

            if (sinhVien == null)
            {
                return Unauthorized(new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "Không tìm thấy hồ sơ sinh viên. Vui lòng liên hệ quản trị viên."
                });
            }

            return null; // Không có lỗi
        }

        // Helper: Chuyển trạng thái số sang chuỗi
        private string GetTrangThaiBuoiHoc(byte? trangThai)
        {
            return trangThai switch
            {
                0 => "Đã hủy",
                1 => "Bình thường",
                2 => "Đổi giờ/phòng",
                3 => "Online",
                4 => "Đổi phòng",
                _ => "Không xác định"
            };
        }

        // Helper: Chuyển loại buổi học sang tiếng Việt
        private string GetLoaiBuoiHoc(string? loaiBuoiHoc)
        {
            return loaiBuoiHoc?.ToLower() switch
            {
                "giang bai" => "Giảng bài",
                "lab" => "Thực hành",
                "tutorial" => "Hướng dẫn",
                "online" => "Trực tuyến",
                _ => "Giảng bài"
            };
        }

        // Helper: Tính tuần học dựa trên ngày
        private (DateTime startDate, DateTime endDate) TinhTuanHoc(int tuan, DateTime ngayBatDauHocKy)
        {
            // Đảm bảo ngày bắt đầu học kỳ là thứ 2
            var ngayBatDau = ngayBatDauHocKy;
            while (ngayBatDau.DayOfWeek != DayOfWeek.Monday)
            {
                ngayBatDau = ngayBatDau.AddDays(-1);
            }

            // Tuần bắt đầu từ thứ 2
            var startDate = ngayBatDau.AddDays((tuan - 1) * 7);
            var endDate = startDate.AddDays(6); // Kết thúc vào Chủ nhật
            return (startDate, endDate);
        }

        // Helper: Lấy thứ trong tuần bằng tiếng Việt
        private string GetThuTiengViet(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => "Không xác định"
            };
        }

        // Helper: Chuyển đổi string thứ sang DayOfWeek
        private DayOfWeek? ConvertThuToDayOfWeek(string thu)
        {
            return thu?.ToLower() switch
            {
                "monday" or "thứ 2" => DayOfWeek.Monday,
                "tuesday" or "thứ 3" => DayOfWeek.Tuesday,
                "wednesday" or "thứ 4" => DayOfWeek.Wednesday,
                "thursday" or "thứ 5" => DayOfWeek.Thursday,
                "friday" or "thứ 6" => DayOfWeek.Friday,
                "saturday" or "thứ 7" => DayOfWeek.Saturday,
                "sunday" or "chủ nhật" => DayOfWeek.Sunday,
                _ => null
            };
        }

        // Helper: Tính ngày dựa trên thứ và tuần (startDateOfWeek đã là thứ 2)
        private DateTime TinhNgayTheoThuVaTuan(DayOfWeek thu, DateTime startDateOfWeek)
        {
            // Đảm bảo startDateOfWeek là thứ 2
            var ngayTrongTuan = startDateOfWeek;
            while (ngayTrongTuan.DayOfWeek != DayOfWeek.Monday)
            {
                ngayTrongTuan = ngayTrongTuan.AddDays(-1);
            }

            // Tính offset từ thứ 2 đến thứ cần tìm
            int offset = thu - DayOfWeek.Monday;
            if (offset < 0) offset += 7;

            return ngayTrongTuan.AddDays(offset);
        }

        // 0. GET: /api/sinhvien/thoi-khoa-bieu/lich-thang/{year}/{month}
        // Lấy danh sách tuần trong tháng (theo lịch thực tế)
        [HttpGet("lich-thang/{year}/{month}")]
        public IActionResult GetDanhSachTuan(int year, int month)
        {
            try
            {
                List<TuanTrongThangDto> weeks = new List<TuanTrongThangDto>();

                DateTime firstDay = new DateTime(year, month, 1);

                // Tìm thứ 2 đầu tiên của tuần chứa ngày 1
                int delta = DayOfWeek.Monday - firstDay.DayOfWeek;
                if (delta > 0) delta -= 7;

                DateTime startOfWeek = firstDay.AddDays(delta);

                int weekNumber = 1;

                while ((startOfWeek.Month == month || startOfWeek.AddDays(6).Month == month)
                    && startOfWeek.Year <= year + 1)
                {
                    var endOfWeek = startOfWeek.AddDays(6);

                    weeks.Add(new TuanTrongThangDto
                    {
                        SoTuan = weekNumber++,
                        StartDate = startOfWeek,
                        EndDate = endOfWeek
                    });

                    startOfWeek = startOfWeek.AddDays(7);
                }

                return Ok(new ApiResponseDTO<List<TuanTrongThangDto>>
                {
                    Success = true,
                    Data = weeks
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 1. POST: /api/sinhvien/thoi-khoa-bieu/tuan
        // Lấy thời khóa biểu theo tuần dựa trên StartDate và EndDate
        [HttpPost("tuan")]
        public async Task<IActionResult> GetThoiKhoaBieuTuan([FromBody] TuanRequestDto req)
        {
            try
            {
                // Kiểm tra quyền truy cập
                var accessCheck = await CheckSinhVienAccess();
                if (accessCheck != null)
                {
                    return accessCheck;
                }

                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Chuyển StartDate và EndDate sang DateTime với thời gian
                DateTime startDate = req.StartDate.Date;
                DateTime endDate = req.EndDate.Date.AddDays(1).AddSeconds(-1); // Kết thúc vào 23:59:59

                // Lấy danh sách lớp học của sinh viên
                var lopHocIds = await _context.SinhVienLops
                    .Where(svl => svl.SinhVienId == sinhVienId)
                    .Select(svl => svl.LopHocId)
                    .ToListAsync();

                if (!lopHocIds.Any())
                {
                    return Ok(new ApiResponseDTO<ThoiKhoaBieuTuanResponseDTO>
                    {
                        Success = true,
                        Data = new ThoiKhoaBieuTuanResponseDTO
                        {
                            TuNgay = startDate.ToString("dd/MM/yyyy"),
                            DenNgay = endDate.ToString("dd/MM/yyyy"),
                            ThoiKhoaBieu = new List<ThoiKhoaBieuNgayDTO>()
                        }
                    });
                }

                // Lấy tất cả buổi học của sinh viên
                var buoiHocList = await _context.BuoiHocs
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(bh => bh.PhongHoc)
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                    .Where(bh => lopHocIds.Contains(bh.LopHocId))
                    .ToListAsync();

                // Lọc buổi học theo ngày trong tuần (dựa trên Thứ và khoảng thời gian)
                var buoiHocTrongTuan = new List<BuoiHoc>();
                foreach (var buoiHoc in buoiHocList)
                {
                    var thu = ConvertThuToDayOfWeek(buoiHoc.Thứ);
                    if (!thu.HasValue) continue;

                    // Tìm tất cả các ngày trong khoảng có cùng thứ
                    var checkDate = startDate;
                    bool foundInRange = false;
                    while (checkDate <= endDate && !foundInRange)
                    {
                        if (checkDate.DayOfWeek == thu.Value)
                        {
                            // Nếu SoBuoi null hoặc bằng 0, buổi học diễn ra hàng tuần
                            if (!buoiHoc.SoBuoi.HasValue || buoiHoc.SoBuoi == 0)
                            {
                                buoiHocTrongTuan.Add(buoiHoc);
                                foundInRange = true; // Chỉ thêm một lần
                            }
                        }
                        checkDate = checkDate.AddDays(1);
                    }
                }

                // Tạo dữ liệu TKB theo ngày
                var thoiKhoaBieu = new List<ThoiKhoaBieuNgayDTO>();
                var ngayTrongTuan = startDate;

                for (int i = 0; i < 7; i++)
                {
                    var ngayHoc = ngayTrongTuan.AddDays(i);
                    var thu = GetThuTiengViet(ngayHoc.DayOfWeek);

                    // Lọc buổi học theo ngày (so sánh trực tiếp thứ)
                    var buoiHocTrongNgay = buoiHocTrongTuan
                        .Where(bh =>
                        {
                            var buoiHocThu = ConvertThuToDayOfWeek(bh.Thứ);
                            return buoiHocThu.HasValue && buoiHocThu.Value == ngayHoc.DayOfWeek;
                        })
                        .OrderBy(bh => bh.ThoiGianBatDau) // Sắp xếp theo giờ bắt đầu
                        .Select(bh => new BuoiHocTKBItemDTO
                        {
                            Id = bh.BuoiHocId,
                            MaMon = bh.LopHoc?.MonHoc?.MaMon,
                            TenMon = bh.LopHoc?.MonHoc?.TenMon,
                            Phong = bh.PhongHoc != null 
                                ? (bh.PhongHoc.TenPhong ?? "Chưa có phòng")
                                : (bh.LoaiBuoiHoc?.ToLower() == "online" ? "Online" : "Chưa có phòng"),
                            GioBatDau = bh.ThoiGianBatDau.ToString("HH:mm"),
                            GioKetThuc = bh.ThoiGianKetThuc.ToString("HH:mm"),
                            ThoiGian = $"{bh.ThoiGianBatDau:HH:mm} - {bh.ThoiGianKetThuc:HH:mm}",
                            TrangThai = GetTrangThaiBuoiHoc(bh.TrangThai),
                            LoaiBuoiHoc = bh.LoaiBuoiHoc?.ToLower() == "online" ? "online" : "offline",
                            LopHoc = bh.LopHoc?.TenLop
                        })
                        .ToList();

                    thoiKhoaBieu.Add(new ThoiKhoaBieuNgayDTO
                    {
                        Thu = thu,
                        Ngay = ngayHoc.ToString("dd/MM/yyyy"),
                        BuoiHoc = buoiHocTrongNgay,
                        TongSoBuoi = buoiHocTrongNgay.Count
                    });
                }

                var response = new ThoiKhoaBieuTuanResponseDTO
                {
                    TuNgay = startDate.ToString("dd/MM/yyyy"),
                    DenNgay = endDate.ToString("dd/MM/yyyy"),
                    ThoiKhoaBieu = thoiKhoaBieu
                };

                return Ok(new ApiResponseDTO<ThoiKhoaBieuTuanResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 2. GET: /api/sinhvien/thoi-khoa-bieu/hien-tai
        [HttpGet("hien-tai")]
        public async Task<IActionResult> GetThoiKhoaBieuHienTai()
        {
            try
            {
                // Tính tuần hiện tại (thứ 2 đến chủ nhật)
                DateTime today = DateTime.Now;
                int delta = DayOfWeek.Monday - today.DayOfWeek;
                if (delta > 0) delta -= 7;
                DateTime startOfWeek = today.AddDays(delta).Date;
                DateTime endOfWeek = startOfWeek.AddDays(6).Date;

                var request = new TuanRequestDto
                {
                    StartDate = startOfWeek,
                    EndDate = endOfWeek
                };

                return await GetThoiKhoaBieuTuan(request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 3. GET: /api/sinhvien/thoi-khoa-bieu/ngay/{date}
        [HttpGet("ngay/{date}")]
        public async Task<IActionResult> GetThoiKhoaBieuNgay(string date)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Parse ngày từ string
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngay))
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Định dạng ngày không hợp lệ. Vui lòng sử dụng định dạng yyyy-MM-dd"
                    });
                }

                // Lấy danh sách lớp học của sinh viên
                var lopHocIds = await _context.SinhVienLops
                    .Where(svl => svl.SinhVienId == sinhVienId)
                    .Select(svl => svl.LopHocId)
                    .ToListAsync();

                // Lấy buổi học - SỬA: BuoiHocs thay vì BuoiHoc
                var buoiHocList = await _context.BuoiHocs
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(bh => bh.PhongHoc)
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                    .Where(bh => lopHocIds.Contains(bh.LopHocId))
                    .ToListAsync();

                // Lọc buổi học theo ngày
                var buoiHocTrongNgay = new List<BuoiHoc>();
                foreach (var buoiHoc in buoiHocList)
                {
                    var thu = ConvertThuToDayOfWeek(buoiHoc.Thứ);
                    if (thu.HasValue && thu.Value == ngay.DayOfWeek)
                    {
                        buoiHocTrongNgay.Add(buoiHoc);
                    }
                }

                var buoiHocResponse = buoiHocTrongNgay.Select(bh => new BuoiHocTKBItemDTO
                {
                    Id = bh.BuoiHocId,
                    MaMon = bh.LopHoc?.MonHoc?.MaMon,
                    TenMon = bh.LopHoc?.MonHoc?.TenMon,
                    Phong = bh.PhongHoc?.TenPhong ?? "Chưa có phòng",
                    GioBatDau = bh.ThoiGianBatDau.ToString("HH:mm"),
                    GioKetThuc = bh.ThoiGianKetThuc.ToString("HH:mm"),
                    ThoiGian = $"{bh.ThoiGianBatDau:HH:mm} - {bh.ThoiGianKetThuc:HH:mm}",
                    TrangThai = GetTrangThaiBuoiHoc(bh.TrangThai),
                    LoaiBuoiHoc = bh.LoaiBuoiHoc?.ToLower() == "online" ? "online" : "offline",
                    LopHoc = bh.LopHoc?.TenLop
                }).ToList();

                var response = new BuoiHocHomNayResponseDTO
                {
                    Ngay = ngay.ToString("dd/MM/yyyy"),
                    Thu = GetThuTiengViet(ngay.DayOfWeek),
                    BuoiHoc = buoiHocResponse,
                    TongSoBuoi = buoiHocResponse.Count
                };

                return Ok(new ApiResponseDTO<BuoiHocHomNayResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 4. GET: /api/sinhvien/thoi-khoa-bieu/buoi-hoc/{id}
        [HttpGet("buoi-hoc/{id}")]
        public async Task<IActionResult> GetChiTietBuoiHoc(int id)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Lấy thông tin buổi học - SỬA: BuoiHocs thay vì BuoiHoc
                var buoiHoc = await _context.BuoiHocs
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(bh => bh.PhongHoc)
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.Khoa)
                    .FirstOrDefaultAsync(bh => bh.BuoiHocId == id);

                if (buoiHoc == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy buổi học"
                    });
                }

                // Kiểm tra sinh viên có thuộc lớp này không
                var sinhVienThuocLop = await _context.SinhVienLops
                    .AnyAsync(svl => svl.SinhVienId == sinhVienId && svl.LopHocId == buoiHoc.LopHocId);

                if (!sinhVienThuocLop)
                {
                    return Forbid();
                }

                // Chu kỳ tuần (giả định từ buổi 1 đến 15)
                var chuKyTuan = $"Tuần {buoiHoc.SoBuoi}";

                // Xử lý địa điểm
                var diaDiem = new DiaDiemDTO();
                if (buoiHoc.PhongHoc != null)
                {
                    // Giả định tên phòng có dạng "Cơ sở 1 - TC - Phòng 201"
                    var parts = buoiHoc.PhongHoc.TenPhong?.Split('-');
                    if (parts != null && parts.Length >= 3)
                    {
                        diaDiem.CoSo = parts[0].Trim();
                        diaDiem.ToaNha = parts[1].Trim();
                        diaDiem.PhongHoc = parts[2].Trim();
                    }
                    else
                    {
                        diaDiem.PhongHoc = buoiHoc.PhongHoc.TenPhong;
                    }
                }

                // Xử lý giảng viên
                var giangVien = new GiangVienDTO();
                if (buoiHoc.LopHoc?.GiangVien?.NguoiDung != null)
                {
                    giangVien.Ten = buoiHoc.LopHoc.GiangVien.NguoiDung.HoTen;
                    giangVien.Email = buoiHoc.LopHoc.GiangVien.NguoiDung.Email;
                }

                var response = new ChiTietBuoiHocDTO
                {
                    Id = buoiHoc.BuoiHocId,
                    MaMon = buoiHoc.LopHoc?.MonHoc?.MaMon,
                    TenMon = buoiHoc.LopHoc?.MonHoc?.TenMon,
                    Thu = buoiHoc.Thứ,
                    GioBatDau = buoiHoc.ThoiGianBatDau.ToString("HH:mm"),
                    GioKetThuc = buoiHoc.ThoiGianKetThuc.ToString("HH:mm"),
                    TrangThai = GetTrangThaiBuoiHoc(buoiHoc.TrangThai),
                    DiaDiem = diaDiem,
                    GiangVien = giangVien,
                    ChuKyTuan = chuKyTuan,
                    ThoiGianBatDau = buoiHoc.LopHoc?.NgayBatDau?.ToString("yyyy-MM-dd"),
                    ThoiGianKetThuc = buoiHoc.LopHoc?.NgayKetThuc?.ToString("yyyy-MM-dd"),
                    GhiChu = buoiHoc.GhiChu
                };

                return Ok(new ApiResponseDTO<ChiTietBuoiHocDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 5. GET: /api/sinhvien/thoi-khoa-bieu/lich-thi
        [HttpGet("lich-thi")]
        public async Task<IActionResult> GetLichThi()
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Lấy danh sách lớp học của sinh viên
                var lopHocIds = await _context.SinhVienLops
                    .Where(svl => svl.SinhVienId == sinhVienId)
                    .Select(svl => svl.LopHocId)
                    .ToListAsync();

                // Lấy lịch thi từ các lớp học - SỬA: BuoiThis thay vì BuoiThi
                var lichThiList = await _context.BuoiThis
                    .Include(bt => bt.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(bt => bt.PhongHoc)
                    .Include(bt => bt.GiamThi)
                        .ThenInclude(g => g.NguoiDung)
                    .Include(bt => bt.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                    .Where(bt => lopHocIds.Contains(bt.LopHocId) &&
                                 bt.NgayThi >= DateTime.Now.Date) // Chỉ lấy lịch thi trong tương lai
                    .OrderBy(bt => bt.NgayThi)
                    .ThenBy(bt => bt.GioBatDau)
                    .ToListAsync();

                var lichThiResponse = lichThiList.Select(bt => new LichThiItemDTO
                {
                    Id = bt.BuoiThiId,
                    MonThi = bt.LopHoc?.MonHoc?.TenMon,
                    MaMon = bt.LopHoc?.MonHoc?.MaMon,
                    Lop = bt.LopHoc?.TenLop,
                    NgayThi = bt.NgayThi.ToString("dd/MM/yyyy"),
                    GioBatDau = bt.GioBatDau?.ToString(@"hh\:mm"),
                    GioKetThuc = bt.GioKetThuc?.ToString(@"hh\:mm"),
                    GiangVien = bt.GiamThi?.NguoiDung?.HoTen ?? bt.LopHoc?.GiangVien?.NguoiDung?.HoTen,
                    BaiThi = "Giữa kỳ", // Có thể xác định dựa trên loại bài kiểm tra
                    HinhThuc = bt.HinhThuc, 
                    PhongThi = bt.PhongHoc?.TenPhong
                }).ToList();

                var response = new LichThiResponseDTO
                {
                    LichThi = lichThiResponse
                };

                return Ok(new ApiResponseDTO<LichThiResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 6. GET: /api/sinhvien/thoi-khoa-bieu/lich-thi/{id}
        [HttpGet("lich-thi/{id}")]
        public async Task<IActionResult> GetChiTietLichThi(int id)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Lấy chi tiết lịch thi - SỬA: BuoiThis thay vì BuoiThi
                var buoiThi = await _context.BuoiThis
                    .Include(bt => bt.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(bt => bt.PhongHoc)
                    .Include(bt => bt.GiamThi)
                        .ThenInclude(g => g.NguoiDung)
                    .FirstOrDefaultAsync(bt => bt.BuoiThiId == id);

                if (buoiThi == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy lịch thi"
                    });
                }

                // Kiểm tra sinh viên có thuộc lớp này không
                var sinhVienThuocLop = await _context.SinhVienLops
                    .AnyAsync(svl => svl.SinhVienId == sinhVienId && svl.LopHocId == buoiThi.LopHocId);

                if (!sinhVienThuocLop)
                {
                    return Forbid();
                }

                var response = new LichThiItemDTO
                {
                    Id = buoiThi.BuoiThiId,
                    MonThi = buoiThi.LopHoc?.MonHoc?.TenMon,
                    MaMon = buoiThi.LopHoc?.MonHoc?.MaMon,
                    Lop = buoiThi.LopHoc?.TenLop,
                    NgayThi = buoiThi.NgayThi.ToString("dd/MM/yyyy"),
                    GioBatDau = buoiThi.GioBatDau?.ToString(@"hh\:mm"),
                    GioKetThuc = buoiThi.GioKetThuc?.ToString(@"hh\:mm"),
                    GiangVien = buoiThi.GiamThi?.NguoiDung?.HoTen ?? "Chưa có giám thị",
                    BaiThi = "Giữa kỳ", // Có thể lấy từ bảng BaiKiemTra
                    HinhThuc = buoiThi.HinhThuc,
                    PhongThi = buoiThi.PhongHoc?.TenPhong
                };

                return Ok(new ApiResponseDTO<LichThiItemDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 7. POST: /api/sinhvien/thoi-khoa-bieu/tuan/danh-sach
        // Lấy danh sách buổi học theo tuần dựa trên StartDate và EndDate
        [HttpPost("tuan/danh-sach")]
        public async Task<IActionResult> GetDanhSachBuoiHocTuan([FromBody] TuanRequestDto req)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Lấy danh sách lớp học của sinh viên
                var lopHocIds = await _context.SinhVienLops
                    .Where(svl => svl.SinhVienId == sinhVienId)
                    .Select(svl => svl.LopHocId)
                    .ToListAsync();

                // Chuyển StartDate và EndDate sang DateTime với thời gian
                DateTime startDate = req.StartDate.Date;
                DateTime endDate = req.EndDate.Date.AddDays(1).AddSeconds(-1);

                if (!lopHocIds.Any())
                {
                    return Ok(new ApiResponseDTO<DanhSachBuoiHocTuanResponseDTO>
                    {
                        Success = true,
                        Data = new DanhSachBuoiHocTuanResponseDTO
                        {
                            TuNgay = startDate.ToString("dd/MM/yyyy"),
                            DenNgay = endDate.ToString("dd/MM/yyyy"),
                            DanhSachBuoiHoc = new List<BuoiHocDanhSachDTO>()
                        }
                    });
                }

                // Lấy tất cả buổi học của sinh viên
                var buoiHocList = await _context.BuoiHocs
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(bh => bh.PhongHoc)
                    .Include(bh => bh.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                    .Where(bh => lopHocIds.Contains(bh.LopHocId))
                    .ToListAsync();

                // Lọc buổi học theo tuần (dựa trên Thứ và khoảng thời gian)
                var buoiHocTrongTuan = new List<BuoiHoc>();
                foreach (var buoiHoc in buoiHocList)
                {
                    var thu = ConvertThuToDayOfWeek(buoiHoc.Thứ);
                    if (!thu.HasValue) continue;

                    // Tìm tất cả các ngày trong khoảng có cùng thứ
                    var checkDate = startDate;
                    bool foundInRange = false;
                    while (checkDate <= endDate && !foundInRange)
                    {
                        if (checkDate.DayOfWeek == thu.Value)
                        {
                            // Nếu SoBuoi null hoặc bằng 0, buổi học diễn ra hàng tuần
                            if (!buoiHoc.SoBuoi.HasValue || buoiHoc.SoBuoi == 0)
                            {
                                buoiHocTrongTuan.Add(buoiHoc);
                                foundInRange = true; // Chỉ thêm một lần
                            }
                        }
                        checkDate = checkDate.AddDays(1);
                    }
                }

                var danhSachBuoiHoc = buoiHocTrongTuan.Select(bh =>
                {
                    // Tạo chuỗi địa điểm
                    var diaDiem = "Chưa có địa điểm";
                    if (bh.PhongHoc != null)
                    {
                        diaDiem = bh.PhongHoc.TenPhong ?? "Chưa có phòng";
                        if (bh.PhongHoc.DiaChi != null)
                        {
                            diaDiem = $"{bh.PhongHoc.DiaChi} - {diaDiem}";
                        }
                    }

                    // Tạo chuỗi thời gian
                    var thuTiengViet = GetThuTiengViet(ConvertThuToDayOfWeek(bh.Thứ) ?? DayOfWeek.Monday);
                    var thoiGian = $"{thuTiengViet} - {bh.ThoiGianBatDau:HH:mm} - {bh.ThoiGianKetThuc:HH:mm}";

                    return new BuoiHocDanhSachDTO
                    {
                        Id = bh.BuoiHocId,
                        MaMon = bh.LopHoc?.MonHoc?.MaMon,
                        TenMon = bh.LopHoc?.MonHoc?.TenMon,
                        TrangThai = GetTrangThaiBuoiHoc(bh.TrangThai),
                        ThoiGian = thoiGian,
                        ChuKyTuan = $"Tuần {bh.SoBuoi}",
                        DiaDiem = diaDiem,
                        GiangVien = bh.LopHoc?.GiangVien?.NguoiDung?.HoTen ?? "Chưa có giảng viên",
                        EmailGiangVien = bh.LopHoc?.GiangVien?.NguoiDung?.Email,
                        LoaiBuoiHoc = GetLoaiBuoiHoc(bh.LoaiBuoiHoc)
                    };
                }).ToList();

                var response = new DanhSachBuoiHocTuanResponseDTO
                {
                    TuNgay = startDate.ToString("dd/MM/yyyy"),
                    DenNgay = endDate.ToString("dd/MM/yyyy"),
                    DanhSachBuoiHoc = danhSachBuoiHoc
                };

                return Ok(new ApiResponseDTO<DanhSachBuoiHocTuanResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 8. GET: /api/sinhvien/thoi-khoa-bieu/hom-nay
        [HttpGet("hom-nay")]
        public async Task<IActionResult> GetThoiKhoaBieuHomNay()
        {
            try
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                return await GetThoiKhoaBieuNgay(today);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}