using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_GV.Models.Data;
using LMS_GV.DTOs.SinhVien;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LMS_GV.Controllers.SinhVien
{
    [Route("api/sinhvien/dang-ky-mon-hoc")]
    [ApiController]
    [Authorize]
    public class SinhVienDangKyMonHocController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SinhVienDangKyMonHocController> _logger;

        public SinhVienDangKyMonHocController(AppDbContext context, ILogger<SinhVienDangKyMonHocController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper method để lấy thông tin sinh viên từ token
        private async Task<Models.HoSoSinhVien?> GetCurrentSinhVienAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                var username = User.FindFirstValue(ClaimTypes.Name);
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(nd => nd.TenDangNhap == username);

                if (nguoiDung != null)
                {
                    userId = nguoiDung.NguoiDungId.ToString();
                }
            }

            if (int.TryParse(userId, out int nguoiDungId))
            {
                return await _context.HoSoSinhViens
                    .Include(hs => hs.NguoiDung)
                    .FirstOrDefaultAsync(hs => hs.NguoiDungId == nguoiDungId);
            }

            return null;
        }

        // Helper method để chuyển DateOnly? sang DateTime?
        private DateTime? ConvertDateOnlyToDateTime(DateOnly? dateOnly)
        {
            if (dateOnly.HasValue)
            {
                return new DateTime(dateOnly.Value.Year, dateOnly.Value.Month, dateOnly.Value.Day);
            }
            return null;
        }

        // 1. GET: /api/sinhvien/dang-ky-mon-hoc/danh-sach
        [HttpGet("danh-sach")]
        public async Task<ActionResult<ApiResponse<object>>> GetDanhSachMonDangKy(
            [FromQuery] int hocKyId = 0)
        {
            try
            {
                var sinhVien = await GetCurrentSinhVienAsync();
                if (sinhVien == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Nếu không có hocKyId, lấy học kỳ hiện tại (học kỳ có ngày bắt đầu gần nhất trong tương lai hoặc hiện tại)
                if (hocKyId == 0)
                {
                    // Tìm học kỳ hiện tại dựa trên ngày hiện tại
                    var currentDate = DateOnly.FromDateTime(DateTime.Now);

                    // Tìm học kỳ đang diễn ra (có lớp học mà ngày hiện tại nằm trong khoảng)
                    var hocKyDienRa = await _context.LopHocs
                        .Where(lh => lh.NgayBatDau <= currentDate && lh.NgayKetThuc >= currentDate)
                        .Select(lh => lh.HocKyId)
                        .Distinct()
                        .FirstOrDefaultAsync();

                    if (hocKyDienRa != 0)
                    {
                        hocKyId = hocKyDienRa;
                    }
                    else
                    {
                        // Nếu không có học kỳ đang diễn ra, lấy học kỳ sắp tới (ngày bắt đầu gần nhất trong tương lai)
                        var hocKySapToi = await _context.LopHocs
                            .Where(lh => lh.NgayBatDau > currentDate)
                            .OrderBy(lh => lh.NgayBatDau)
                            .Select(lh => lh.HocKyId)
                            .FirstOrDefaultAsync();

                        if (hocKySapToi != 0)
                        {
                            hocKyId = hocKySapToi;
                        }
                        else
                        {
                            // Nếu vẫn không có, lấy học kỳ gần nhất (theo ID lớn nhất)
                            var latestHocKy = await _context.HocKies
                                .OrderByDescending(hk => hk.HocKyId)
                                .FirstOrDefaultAsync();

                            if (latestHocKy != null)
                                hocKyId = latestHocKy.HocKyId;
                        }
                    }
                }

                _logger.LogInformation($"Sinh viên {sinhVien.SinhVienId} - Lấy danh sách môn học cho học kỳ {hocKyId}");

                // Lấy tất cả lớp học trong học kỳ, không lọc theo ngành/khóa
                var lopHocTrongHocKy = await _context.LopHocs
                    .Include(lh => lh.MonHoc)
                    .Include(lh => lh.HocKy)
                    .Include(lh => lh.GiangVien)
                    .Where(lh => lh.HocKyId == hocKyId && lh.TrangThai == 1)
                    .Select(lh => new
                    {
                        lh,
                        lh.MonHoc,
                        lh.HocKy,
                        lh.GiangVien
                    })
                    .ToListAsync();

                _logger.LogInformation($"Tìm thấy {lopHocTrongHocKy.Count} lớp học trong học kỳ {hocKyId}");

                // Lấy danh sách môn đã đăng ký của sinh viên trong học kỳ này
                var monDaDangKy = await _context.DangKyTinChis
                    .Where(dk => dk.SinhVienId == sinhVien.SinhVienId
                        && dk.HocKyId == hocKyId
                        && dk.TrangThai == 2) // Đã đăng ký thành công
                    .Select(dk => dk.MonHocId)
                    .ToListAsync();

                _logger.LogInformation($"Sinh viên đã đăng ký {monDaDangKy.Count} môn trong học kỳ {hocKyId}");

                // Lấy thông tin giảng viên cho tất cả lớp học
                var giangVienIds = lopHocTrongHocKy
                    .Where(item => item.lh.GiangVienId.HasValue)
                    .Select(item => item.lh.GiangVienId.Value)
                    .Distinct()
                    .ToList();

                var giangVienInfo = await _context.GiangViens
                    .Include(gv => gv.NguoiDung)
                    .Where(gv => giangVienIds.Contains(gv.GiangVienId))
                    .Select(gv => new
                    {
                        gv.GiangVienId,
                        gv.NguoiDung.HoTen,
                        gv.NguoiDung.Email
                    })
                    .ToDictionaryAsync(gv => gv.GiangVienId);

                var result = new List<MonHocDangKyDTO>();

                foreach (var item in lopHocTrongHocKy)
                {
                    var isRegistered = monDaDangKy.Contains(item.lh.MonHocId);

                    string? giangVienHoTen = "Chưa phân công";
                    string? giangVienEmail = "";

                    if (item.lh.GiangVienId.HasValue && giangVienInfo.ContainsKey(item.lh.GiangVienId.Value))
                    {
                        giangVienHoTen = giangVienInfo[item.lh.GiangVienId.Value].HoTen;
                        giangVienEmail = giangVienInfo[item.lh.GiangVienId.Value].Email;
                    }

                    result.Add(new MonHocDangKyDTO
                    {
                        Id = item.lh.MonHocId,
                        MaMon = item.MonHoc.MaMon,
                        TenMon = item.MonHoc.TenMon,
                        GiangVien = giangVienHoTen,
                        EmailGiangVien = giangVienEmail,
                        HocKy = item.HocKy.KiHoc,
                        NamHoc = item.HocKy.NamHoc,
                        ThoiGianBatDau = ConvertDateOnlyToDateTime(item.lh.NgayBatDau),
                        ThoiGianKetThuc = ConvertDateOnlyToDateTime(item.lh.NgayKetThuc),
                        SoTinChi = item.lh.SoTinChi,
                        TrangThai = isRegistered ? "Đã đăng ký" : "Chưa đăng ký",
                        LopHocId = item.lh.LopHocId,
                        MaLop = item.lh.MaLop
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { DanhSachMonHoc = result }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách môn đăng ký");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi xử lý yêu cầu"
                });
            }
        }

        // 2. GET: /api/sinhvien/dang-ky-mon-hoc/{maMon}/chi-tiet
        [HttpGet("{maMon}/chi-tiet")]
        public async Task<ActionResult<ApiResponse<ChiTietMonHocDTO>>> GetChiTietMonHoc(string maMon)
        {
            try
            {
                var monHoc = await _context.MonHocs
                    .FirstOrDefaultAsync(mh => mh.MaMon == maMon);

                if (monHoc == null)
                {
                    return NotFound(new ApiResponse<ChiTietMonHocDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy môn học"
                    });
                }

                // Lấy lớp học đầu tiên của môn này
                var lopHoc = await _context.LopHocs
                    .Include(lh => lh.HocKy)
                    .Include(lh => lh.GiangVien)
                    .Where(lh => lh.MonHocId == monHoc.MonHocId && lh.TrangThai == 1)
                    .Select(lh => new
                    {
                        lh,
                        lh.HocKy,
                        lh.GiangVien
                    })
                    .FirstOrDefaultAsync();

                if (lopHoc == null)
                {
                    return NotFound(new ApiResponse<ChiTietMonHocDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy lớp học cho môn này"
                    });
                }

                // Lấy thông tin buổi học đầu tiên
                var buoiHoc = await _context.BuoiHocs
                    .Include(bh => bh.PhongHoc)
                    .Where(bh => bh.LopHocId == lopHoc.lh.LopHocId)
                    .OrderBy(bh => bh.ThoiGianBatDau)
                    .FirstOrDefaultAsync();

                // Lấy thông tin giảng viên từ NguoiDung
                var nguoiDungGV = lopHoc.lh.GiangVienId.HasValue ?
                    await _context.GiangViens
                        .Where(gv => gv.GiangVienId == lopHoc.lh.GiangVienId.Value)
                        .Select(gv => gv.NguoiDung)
                        .FirstOrDefaultAsync() : null;

                var result = new ChiTietMonHocDTO
                {
                    MaMon = monHoc.MaMon,
                    TenMon = monHoc.TenMon,
                    GiangVien = nguoiDungGV != null ? nguoiDungGV.HoTen : "Chưa phân công",
                    EmailGiangVien = nguoiDungGV != null ? nguoiDungGV.Email : "",
                    ThoiGianHoc = buoiHoc != null ?
                        $"{buoiHoc.Thứ}, {buoiHoc.ThoiGianBatDau:HH:mm}-{buoiHoc.ThoiGianKetThuc:HH:mm}" : "Chưa có lịch",
                    DiaDiem = buoiHoc != null && buoiHoc.PhongHoc != null ? buoiHoc.PhongHoc.TenPhong : "Chưa xác định",
                    TinChi = lopHoc.lh.SoTinChi,
                    TongBuoi = lopHoc.lh.SoTiet.HasValue ? lopHoc.lh.SoTiet.Value / 3 : 0, // Giả sử mỗi buổi 3 tiết
                    SoBuoiDuocVang = lopHoc.lh.SoBuoiVangChoPhep,
                    ThoiGianBatDau = ConvertDateOnlyToDateTime(lopHoc.lh.NgayBatDau),
                    ThoiGianKetThuc = ConvertDateOnlyToDateTime(lopHoc.lh.NgayKetThuc),
                    HocKy = lopHoc.HocKy.KiHoc,
                    NamHoc = lopHoc.HocKy.NamHoc
                };

                return Ok(new ApiResponse<ChiTietMonHocDTO>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết môn học");
                return StatusCode(500, new ApiResponse<ChiTietMonHocDTO>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi xử lý yêu cầu"
                });
            }
        }

        // 3. POST: /api/sinhvien/dang-ky-mon-hoc/{maMon}/dang-ky
        [HttpPost("{maMon}/dang-ky")]
        public async Task<ActionResult<ApiResponse<DangKyMonHocResponseDTO>>> DangKyMonHoc(
            string maMon,
            [FromBody] DangKyMonHocRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sinhVien = await GetCurrentSinhVienAsync();
                if (sinhVien == null)
                {
                    return Unauthorized(new ApiResponse<DangKyMonHocResponseDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Tìm môn học
                var monHoc = await _context.MonHocs
                    .FirstOrDefaultAsync(mh => mh.MaMon == maMon);

                if (monHoc == null)
                {
                    return NotFound(new ApiResponse<DangKyMonHocResponseDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy môn học"
                    });
                }

                // Kiểm tra môn đã đăng ký chưa
                var daDangKy = await _context.DangKyTinChis
                    .AnyAsync(dk => dk.SinhVienId == sinhVien.SinhVienId
                        && dk.MonHocId == monHoc.MonHocId
                        && dk.HocKyId == request.HocKyId
                        && dk.TrangThai == 2);

                if (daDangKy)
                {
                    return BadRequest(new ApiResponse<DangKyMonHocResponseDTO>
                    {
                        Success = false,
                        Message = "Bạn đã đăng ký môn học này rồi"
                    });
                }

                // Kiểm tra trùng lịch
                var kiemTraTrungLich = await KiemTraTrungLichAsync(sinhVien.SinhVienId, monHoc.MonHocId, request.HocKyId);
                if (kiemTraTrungLich.CoTrungLich)
                {
                    if (kiemTraTrungLich.ChiTietTrungLich != null && kiemTraTrungLich.ChiTietTrungLich.Any())
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            ErrorCode = "TRUNG_LICH",
                            Message = "Bạn đã trùng lịch học",
                            Data = new
                            {
                                MonTrungLich = kiemTraTrungLich.ChiTietTrungLich.First()
                            }
                        });
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            ErrorCode = "TRUNG_LICH",
                            Message = "Bạn đã trùng lịch học"
                        });
                    }
                }

                // Tìm lớp học phù hợp
                var lopHoc = await TimLopHocPhuHopAsync(monHoc.MonHocId, sinhVien.NganhId, sinhVien.KhoaTuyenSinhId, request.HocKyId);

                if (lopHoc == null)
                {
                    return BadRequest(new ApiResponse<DangKyMonHocResponseDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy lớp học phù hợp hoặc đã đầy"
                    });
                }

                // Tạo đăng ký tín chỉ
                var dangKyTinChi = new Models.DangKyTinChi
                {
                    SinhVienId = sinhVien.SinhVienId,
                    MonHocId = monHoc.MonHocId,
                    HocKyId = request.HocKyId,
                    NgayDangKy = DateTime.Now,
                    TrangThai = 1, // Đã đăng ký
                    CreatedAt = DateTime.Now
                };

                _context.DangKyTinChis.Add(dangKyTinChi);
                await _context.SaveChangesAsync();

                // Thêm sinh viên vào lớp
                var sinhVienLop = new Models.SinhVienLop
                {
                    SinhVienId = sinhVien.SinhVienId,
                    LopHocId = lopHoc.LopHocId,
                    TinhTrang = "dang hoc",
                    CreatedAt = DateTime.Now
                };

                _context.SinhVienLops.Add(sinhVienLop);

                // Cập nhật sĩ số lớp
                lopHoc.SiSo = (lopHoc.SiSo ?? 0) + 1;

                // Cập nhật trạng thái đăng ký thành công
                dangKyTinChi.TrangThai = 2; // Đã đăng ký thành công

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Lấy thông tin giảng viên cho response
                var giangVien = await _context.GiangViens
                    .Where(gv => gv.GiangVienId == lopHoc.GiangVienId)
                    .Select(gv => gv.NguoiDung)
                    .FirstOrDefaultAsync();

                var hocKy = await _context.HocKies.FindAsync(request.HocKyId);

                var result = new DangKyMonHocResponseDTO
                {
                    MaMon = monHoc.MaMon,
                    TenMon = monHoc.TenMon,
                    LopHoc = lopHoc.MaLop,
                    GiangVien = giangVien != null ? giangVien.HoTen : "Chưa phân công",
                    HocKy = hocKy != null ? hocKy.KiHoc : "",
                    ThoiGianBatDau = ConvertDateOnlyToDateTime(lopHoc.NgayBatDau),
                    ThoiGianKetThuc = ConvertDateOnlyToDateTime(lopHoc.NgayKetThuc),
                    TrangThai = "Đã đăng ký thành công"
                };

                return Ok(new ApiResponse<DangKyMonHocResponseDTO>
                {
                    Success = true,
                    Message = "Đăng ký thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi đăng ký môn học");
                return StatusCode(500, new ApiResponse<DangKyMonHocResponseDTO>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi đăng ký môn học"
                });
            }
        }

        // 4. GET: /api/sinhvien/dang-ky-mon-hoc/kiem-tra-trung-lich
        [HttpGet("kiem-tra-trung-lich")]
        public async Task<ActionResult<ApiResponse<KiemTraTrungLichDTO>>> KiemTraTrungLich(
            [FromQuery] string maMon,
            [FromQuery] int hocKyId)
        {
            try
            {
                var sinhVien = await GetCurrentSinhVienAsync();
                if (sinhVien == null)
                {
                    return Unauthorized(new ApiResponse<KiemTraTrungLichDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                var monHoc = await _context.MonHocs
                    .FirstOrDefaultAsync(mh => mh.MaMon == maMon);

                if (monHoc == null)
                {
                    return NotFound(new ApiResponse<KiemTraTrungLichDTO>
                    {
                        Success = false,
                        Message = "Không tìm thấy môn học"
                    });
                }

                var result = await KiemTraTrungLichAsync(sinhVien.SinhVienId, monHoc.MonHocId, hocKyId);

                return Ok(new ApiResponse<KiemTraTrungLichDTO>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra trùng lịch");
                return StatusCode(500, new ApiResponse<KiemTraTrungLichDTO>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi kiểm tra trùng lịch"
                });
            }
        }

        // 5. GET: /api/sinhvien/dang-ky-mon-hoc/da-dang-ky
        [HttpGet("da-dang-ky")]
        public async Task<ActionResult<ApiResponse<object>>> GetMonDaDangKy()
        {
            try
            {
                var sinhVien = await GetCurrentSinhVienAsync();
                if (sinhVien == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Lấy tất cả môn đã đăng ký của sinh viên
                var monDaDangKy = await (from dk in _context.DangKyTinChis
                                         join mh in _context.MonHocs on dk.MonHocId equals mh.MonHocId
                                         join lh in _context.LopHocs on dk.MonHocId equals lh.MonHocId
                                         join gv in _context.GiangViens on lh.GiangVienId equals gv.GiangVienId into gvJoin
                                         from gv in gvJoin.DefaultIfEmpty()
                                         join nd in _context.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId into ndJoin
                                         from nd in ndJoin.DefaultIfEmpty()
                                         join hk in _context.HocKies on dk.HocKyId equals hk.HocKyId
                                         where dk.SinhVienId == sinhVien.SinhVienId
                                             && dk.TrangThai == 2 // Đã đăng ký thành công
                                         select new MonDaDangKyDTO
                                         {
                                             Id = mh.MonHocId,
                                             MaMon = mh.MaMon,
                                             TenMon = mh.TenMon,
                                             GiangVien = nd.HoTen,
                                             HocKy = hk.KiHoc,
                                             ThoiGianBatDau = ConvertDateOnlyToDateTime(lh.NgayBatDau),
                                             ThoiGianKetThuc = ConvertDateOnlyToDateTime(lh.NgayKetThuc),
                                             LopHoc = lh.MaLop,
                                             TrangThai = "Đã đăng ký"
                                         }).ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { DanhSachMonDaDangKy = monDaDangKy }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách môn đã đăng ký");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi xử lý yêu cầu"
                });
            }
        }

        // Helper methods
        private async Task<KiemTraTrungLichDTO> KiemTraTrungLichAsync(int sinhVienId, int monHocId, int hocKyId)
        {
            var result = new KiemTraTrungLichDTO { CoTrungLich = false };

            // Lấy các buổi học của môn muốn đăng ký
            var buoiHocMoi = await (from lh in _context.LopHocs
                                    join bh in _context.BuoiHocs on lh.LopHocId equals bh.LopHocId
                                    where lh.MonHocId == monHocId && lh.HocKyId == hocKyId
                                    select new
                                    {
                                        bh.Thứ,
                                        bh.ThoiGianBatDau,
                                        bh.ThoiGianKetThuc
                                    }).ToListAsync();

            if (!buoiHocMoi.Any())
                return result;

            // Lấy các môn đã đăng ký trong học kỳ
            var monDaDangKy = await (from dk in _context.DangKyTinChis
                                     join lh in _context.LopHocs on dk.MonHocId equals lh.MonHocId
                                     where dk.SinhVienId == sinhVienId
                                         && dk.HocKyId == hocKyId
                                         && dk.TrangThai == 2
                                     select lh.LopHocId).ToListAsync();

            if (!monDaDangKy.Any())
                return result;

            // Lấy buổi học của các môn đã đăng ký
            var buoiHocDaDangKy = await (from bh in _context.BuoiHocs
                                         where monDaDangKy.Contains(bh.LopHocId)
                                         select new
                                         {
                                             bh.Thứ,
                                             bh.ThoiGianBatDau,
                                             bh.ThoiGianKetThuc,
                                             bh.LopHocId
                                         }).ToListAsync();

            var chiTietTrungLich = new List<ChiTietTrungLichDTO>();

            foreach (var buoiMoi in buoiHocMoi)
            {
                foreach (var buoiCu in buoiHocDaDangKy)
                {
                    // Kiểm tra trùng thứ
                    if (NormalizeThu(buoiMoi.Thứ) == NormalizeThu(buoiCu.Thứ))
                    {
                        // Kiểm tra trùng thời gian
                        var start1 = buoiMoi.ThoiGianBatDau.TimeOfDay;
                        var end1 = buoiMoi.ThoiGianKetThuc.TimeOfDay;
                        var start2 = buoiCu.ThoiGianBatDau.TimeOfDay;
                        var end2 = buoiCu.ThoiGianKetThuc.TimeOfDay;

                        if ((start1 < end2 && end1 > start2))
                        {
                            result.CoTrungLich = true;

                            // Lấy thông tin môn bị trùng
                            var lopTrung = await (from lh in _context.LopHocs
                                                  join mh in _context.MonHocs on lh.MonHocId equals mh.MonHocId
                                                  where lh.LopHocId == buoiCu.LopHocId
                                                  select new { lh, mh }).FirstOrDefaultAsync();

                            if (lopTrung != null)
                            {
                                chiTietTrungLich.Add(new ChiTietTrungLichDTO
                                {
                                    MaMon = lopTrung.mh.MaMon,
                                    TenMon = lopTrung.mh.TenMon,
                                    Thu = buoiCu.Thứ,
                                    GioBatDau = start2,
                                    GioKetThuc = end2,
                                    NgayTrung = buoiCu.ThoiGianBatDau.Date
                                });
                            }
                        }
                    }
                }
            }

            result.ChiTietTrungLich = chiTietTrungLich;
            return result;
        }

        private string NormalizeThu(string? thu)
        {
            if (string.IsNullOrEmpty(thu))
                return "";

            var normalized = thu.ToLower()
                .Replace("thu", "")
                .Replace(" ", "")
                .Replace("ứ", "")
                .Trim();

            // Chuyển số tiếng Việt thành số
            var numberMap = new Dictionary<string, string>
            {
                {"hai", "2"}, {"ba", "3"}, {"tư", "4"}, {"nam", "5"},
                {"sau", "6"}, {"bay", "7"}, {"bảy", "7"}, {"tam", "8"},
                {"chin", "9"}, {"muoi", "10"}, {"mười", "10"}
            };

            foreach (var mapping in numberMap)
            {
                if (normalized.Contains(mapping.Key))
                {
                    return mapping.Value;
                }
            }

            // Nếu là số, trả về số
            if (Regex.IsMatch(normalized, @"^\d+$"))
            {
                return normalized;
            }

            return thu.ToLower();
        }

        private async Task<Models.LopHoc?> TimLopHocPhuHopAsync(int monHocId, int? nganhId, int? khoaTuyenSinhId, int hocKyId)
        {
            // Tìm lớp học phù hợp theo thứ tự ưu tiên:
            // 1. Cùng ngành, cùng khóa, chưa đầy
            // 2. Cùng ngành, chưa đầy
            // 3. Bất kỳ lớp nào còn chỗ

            var lopHoc = await _context.LopHocs
                .Where(lh => lh.MonHocId == monHocId
                    && lh.HocKyId == hocKyId
                    && lh.TrangThai == 1
                    && (lh.SiSo < lh.SiSoToiDa || lh.SiSoToiDa == null))
                .OrderByDescending(lh => lh.NganhId == nganhId && lh.KhoaTuyenSinhId == khoaTuyenSinhId)
                .ThenByDescending(lh => lh.NganhId == nganhId)
                .ThenByDescending(lh => lh.KhoaTuyenSinhId == khoaTuyenSinhId)
                .FirstOrDefaultAsync();

            return lopHoc;
        }
    }
}