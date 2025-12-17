using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using LMS_GV.DTOs.TruongKhoa;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LMS_GV.Controllers.TruongKhoa
{
    [Route("api/truong-khoa/lop-hoc")]
    [ApiController]
    [Authorize]
    public class TruongKhoaLopHocController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TruongKhoaLopHocController> _logger;

        public TruongKhoaLopHocController(AppDbContext context, ILogger<TruongKhoaLopHocController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy ID Trưởng khoa từ token
        private int GetTruongKhoaId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng");
        }

        // Lấy Khoa của Trưởng khoa
        private async Task<int?> GetKhoaIdOfTruongKhoa()
        {
            var truongKhoaId = GetTruongKhoaId();
            var giangVien = await _context.GiangViens
                .FirstOrDefaultAsync(gv => gv.NguoiDungId == truongKhoaId && gv.ChucVu == "Trưởng khoa");

            return giangVien?.KhoaId;
        }

        // 1. DANH SÁCH LỚP HỌC
        [HttpGet]
        public async Task<ActionResult<TruongKhoaPagedResponse<LopHocListDTO>>> GetDanhSachLopHoc(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? monHocId = null,
            [FromQuery] int? khoaId = null,
            [FromQuery] int? nganhId = null,
            [FromQuery] int? giangVienId = null,
            [FromQuery] byte? trangThai = null)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new { success = false, message = "Bạn không phải là Trưởng khoa" });
                }

                var query = _context.LopHocs
                    .Include(l => l.MonHoc)
                    .Include(l => l.GiangVien)
                    .ThenInclude(gv => gv.NguoiDung)
                    .Include(l => l.Khoa)
                    .Include(l => l.Nganh)
                    .Where(l => l.KhoaId == khoaIdTruongKhoa.Value);

                // Áp dụng bộ lọc
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(l =>
                        (l.MaLop != null && l.MaLop.Contains(search)) ||
                        (l.TenLop != null && l.TenLop.Contains(search)));
                }

                if (monHocId.HasValue)
                {
                    query = query.Where(l => l.MonHocId == monHocId.Value);
                }

                if (khoaId.HasValue)
                {
                    query = query.Where(l => l.KhoaId == khoaId.Value);
                }

                if (nganhId.HasValue)
                {
                    query = query.Where(l => l.NganhId == nganhId.Value);
                }

                if (giangVienId.HasValue)
                {
                    query = query.Where(l => l.GiangVienId == giangVienId.Value);
                }

                if (trangThai.HasValue)
                {
                    query = query.Where(l => l.TrangThai == trangThai.Value);
                }

                // Phân trang
                var total = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(total / (double)limit);

                var lopHocs = await query
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(l => new LopHocListDTO
                    {
                        LopHocId = l.LopHocId,
                        MaLop = l.MaLop,
                        TenLop = l.TenLop,
                        TenMonHoc = l.MonHoc.TenMon,
                        TenGiangVien = l.GiangVien != null && l.GiangVien.NguoiDung != null ?
                            l.GiangVien.NguoiDung.HoTen : "Chưa phân công",
                        TenKhoa = l.Khoa != null ? l.Khoa.TenKhoa : null,
                        TenNganh = l.Nganh != null ? l.Nganh.TenNganh : null,
                        SiSoHienTai = l.SiSo,
                        SiSoToiDa = l.SiSoToiDa,
                        TrangThai = l.TrangThai == 1 ? "Đang hoạt động" : "Ngưng hoạt động",
                        NgayBatDau = l.NgayBatDau.HasValue ? l.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue) : null,
                        NgayKetThuc = l.NgayKetThuc.HasValue ? l.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : null
                    })
                    .ToListAsync();

                var response = new TruongKhoaPagedResponse<LopHocListDTO>
                {
                    Data = new TruongKhoaPagedData<LopHocListDTO>
                    {
                        Items = lopHocs,
                        Total = total,
                        Page = page,
                        Limit = limit,
                        TotalPages = totalPages
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lớp học");
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }

        // 2. CHI TIẾT LỚP HỌC
        [HttpGet("{id}")]
        public async Task<ActionResult<TruongKhoaApiResponse<LopHocDetailDTO>>> GetChiTietLopHoc(int id)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                var lopHoc = await _context.LopHocs
                    .Include(l => l.MonHoc)
                    .Include(l => l.HocKy)
                    .Include(l => l.GiangVien)
                    .ThenInclude(gv => gv.NguoiDung)
                    .Include(l => l.Khoa)
                    .Include(l => l.Nganh)
                    .Include(l => l.SinhVienLops)
                    .ThenInclude(sl => sl.SinhVien)
                    .ThenInclude(sv => sv.NguoiDung)
                    .Include(l => l.SinhVienLops)
                    .ThenInclude(sl => sl.SinhVien)
                    .ThenInclude(sv => sv.Nganh)
                    .Include(l => l.BuoiHocs)
                    .ThenInclude(bh => bh.PhongHoc)
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                var detailDTO = new LopHocDetailDTO
                {
                    LopHocId = lopHoc.LopHocId,
                    MaLop = lopHoc.MaLop,
                    TenLop = lopHoc.TenLop,
                    MonHoc = new ThongTinMonHocDTO
                    {
                        MonHocId = lopHoc.MonHocId,
                        MaMon = lopHoc.MonHoc.MaMon,
                        TenMon = lopHoc.MonHoc.TenMon,
                        SoTinChi = lopHoc.MonHoc.SoTinChi
                    },
                    HocKy = new ThongTinHocKyDTO
                    {
                        HocKyId = lopHoc.HocKyId,
                        NamHoc = lopHoc.HocKy.NamHoc,
                        KiHoc = lopHoc.HocKy.KiHoc
                    },
                    GiangVien = lopHoc.GiangVien != null && lopHoc.GiangVien.NguoiDung != null ?
                        new ThongTinGiangVienDTO
                        {
                            GiangVienId = lopHoc.GiangVien.GiangVienId,
                            HoTen = lopHoc.GiangVien.NguoiDung.HoTen,
                            Email = lopHoc.GiangVien.NguoiDung.Email,
                            HocVi = lopHoc.GiangVien.HocVi,
                            ChuyenMon = lopHoc.GiangVien.ChuyenMon
                        } : null,
                    SiSoHienTai = lopHoc.SiSo,
                    SiSoToiDa = lopHoc.SiSoToiDa,
                    SoTinChi = lopHoc.SoTinChi,
                    SoTiet = lopHoc.SoTiet,
                    SoBuoiVangChoPhep = lopHoc.SoBuoiVangChoPhep,
                    NgayBatDau = lopHoc.NgayBatDau.HasValue ? lopHoc.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue) : null,
                    NgayKetThuc = lopHoc.NgayKetThuc.HasValue ? lopHoc.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : null,
                    TrangThai = lopHoc.TrangThai == 1 ? "Đang hoạt động" : "Ngưng hoạt động",
                    DanhSachSinhVien = lopHoc.SinhVienLops.Select(sl => new ThongTinSinhVienDTO
                    {
                        SinhVienId = sl.SinhVienId,
                        MSSV = sl.SinhVien.Mssv,
                        HoTen = sl.SinhVien.NguoiDung != null ? sl.SinhVien.NguoiDung.HoTen : null,
                        Email = sl.SinhVien.NguoiDung != null ? sl.SinhVien.NguoiDung.Email : null,
                        TenNganh = sl.SinhVien.Nganh != null ? sl.SinhVien.Nganh.TenNganh : null,
                        TenKhoaTuyenSinh = sl.SinhVien.KhoaTuyenSinh != null ? sl.SinhVien.KhoaTuyenSinh.TenKhoaTuyenSinh : null,
                        GPA = sl.SinhVien.Gpa,
                        TrangThai = sl.TinhTrang,
                        NgayVaoLop = sl.NgayVao
                    }).ToList(),
                    LichDay = lopHoc.BuoiHocs.Select(bh => new LichDayDTO
                    {
                        BuoiHocId = bh.BuoiHocId,
                        Thu = bh.Thứ,
                        GioBatDau = bh.ThoiGianBatDau.TimeOfDay,
                        GioKetThuc = bh.ThoiGianKetThuc.TimeOfDay,
                        TenPhong = bh.PhongHoc != null ? bh.PhongHoc.TenPhong : null,
                        MaPhong = bh.PhongHoc != null ? bh.PhongHoc.MaPhong : null,
                        DiaChi = bh.PhongHoc != null ? bh.PhongHoc.DiaChi : null,
                        LoaiBuoiHoc = bh.LoaiBuoiHoc,
                        SoBuoi = bh.SoBuoi ?? 1,
                        GhiChu = bh.GhiChu
                    }).ToList()
                };

                return Ok(new TruongKhoaApiResponse<LopHocDetailDTO>
                {
                    Success = true,
                    Data = detailDTO
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy chi tiết lớp học ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 3. TẠO LỚP HỌC MỚI
        [HttpPost]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> TaoLopHocMoi([FromBody] TaoLopHocRequestDTO request)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Kiểm tra mã lớp trùng
                if (!string.IsNullOrEmpty(request.MaLop))
                {
                    var existingLop = await _context.LopHocs
                        .FirstOrDefaultAsync(l => l.MaLop == request.MaLop);
                    if (existingLop != null)
                    {
                        return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Mã lớp đã tồn tại" });
                    }
                }

                // Kiểm tra môn học
                var monHoc = await _context.MonHocs.FindAsync(request.MonHocId);
                if (monHoc == null)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Môn học không tồn tại" });
                }

                // Kiểm tra học kỳ
                var hocKy = await _context.HocKies.FindAsync(request.HocKyId);
                if (hocKy == null)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Học kỳ không tồn tại" });
                }

                // Kiểm tra giảng viên (nếu có)
                if (request.GiangVienId.HasValue)
                {
                    var giangVien = await _context.GiangViens.FindAsync(request.GiangVienId.Value);
                    if (giangVien == null)
                    {
                        return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Giảng viên không tồn tại" });
                    }
                }

                var lopHoc = new LopHoc
                {
                    MaLop = request.MaLop,
                    TenLop = request.TenLop,
                    MonHocId = request.MonHocId,
                    HocKyId = request.HocKyId,
                    KhoaTuyenSinhId = request.KhoaTuyenSinhId,
                    KhoaId = khoaIdTruongKhoa.Value,
                    NganhId = request.NganhId,
                    SiSoToiDa = request.SiSoToiDa,
                    SiSo = 0,
                    SoTinChi = request.SoTinChi ?? monHoc.SoTinChi,
                    SoTiet = request.SoTiet,
                    SoBuoiVangChoPhep = request.SoBuoiVangChoPhep,
                    GiangVienId = request.GiangVienId,
                    NgayBatDau = request.NgayBatDau.HasValue ? DateOnly.FromDateTime(request.NgayBatDau.Value) : (DateOnly?)null,
                    NgayKetThuc = request.NgayKetThuc.HasValue ? DateOnly.FromDateTime(request.NgayKetThuc.Value) : (DateOnly?)null,
                    TrangThai = request.TrangThai,
                    CreatedAt = DateTime.Now
                };

                _context.LopHocs.Add(lopHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Tạo lớp học thành công",
                    Data = new
                    {
                        id = lopHoc.LopHocId,
                        maLop = lopHoc.MaLop,
                        tenLop = lopHoc.TenLop
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lớp học mới");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 4. CẬP NHẬT LỚP HỌC
        [HttpPut("{id}")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> CapNhatLopHoc(int id, [FromBody] CapNhatLopHocRequestDTO request)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                // Kiểm tra mã lớp trùng (nếu thay đổi)
                if (!string.IsNullOrEmpty(request.MaLop) && request.MaLop != lopHoc.MaLop)
                {
                    var existingLop = await _context.LopHocs
                        .FirstOrDefaultAsync(l => l.MaLop == request.MaLop && l.LopHocId != id);
                    if (existingLop != null)
                    {
                        return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Mã lớp đã tồn tại" });
                    }
                    lopHoc.MaLop = request.MaLop;
                }

                if (!string.IsNullOrEmpty(request.TenLop))
                    lopHoc.TenLop = request.TenLop;

                if (request.SiSoToiDa.HasValue)
                    lopHoc.SiSoToiDa = request.SiSoToiDa.Value;

                if (request.SoTinChi.HasValue)
                    lopHoc.SoTinChi = request.SoTinChi.Value;

                if (request.SoTiet.HasValue)
                    lopHoc.SoTiet = request.SoTiet.Value;

                if (request.SoBuoiVangChoPhep.HasValue)
                    lopHoc.SoBuoiVangChoPhep = request.SoBuoiVangChoPhep.Value;

                if (request.NgayBatDau.HasValue)
                    lopHoc.NgayBatDau = DateOnly.FromDateTime(request.NgayBatDau.Value);

                if (request.NgayKetThuc.HasValue)
                    lopHoc.NgayKetThuc = DateOnly.FromDateTime(request.NgayKetThuc.Value);

                if (request.TrangThai.HasValue)
                    lopHoc.TrangThai = request.TrangThai.Value;

                lopHoc.UpdatedAt = DateTime.Now;

                _context.LopHocs.Update(lopHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Cập nhật lớp học thành công",
                    Data = new
                    {
                        id = lopHoc.LopHocId,
                        maLop = lopHoc.MaLop,
                        tenLop = lopHoc.TenLop
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật lớp học ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 5. XÓA LỚP HỌC (Xóa mềm - chỉ đổi trạng thái)
        [HttpDelete("{id}")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> XoaLopHoc(int id)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                // Kiểm tra nếu lớp còn sinh viên
                var coSinhVien = await _context.SinhVienLops
                    .AnyAsync(sl => sl.LopHocId == id && sl.TinhTrang == "dang hoc");

                if (coSinhVien)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Không thể xóa lớp đang có sinh viên" });
                }

                // Chỉ đổi trạng thái (xóa mềm)
                lopHoc.TrangThai = 0;
                lopHoc.UpdatedAt = DateTime.Now;

                _context.LopHocs.Update(lopHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa lớp học thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa lớp học ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 6. LẤY THÔNG TIN GIẢNG VIÊN HIỆN TẠI CỦA LỚP
        [HttpGet("{id}/giang-vien")]
        public async Task<ActionResult<TruongKhoaApiResponse<ThongTinGiangVienDTO>>> GetGiangVienHienTai(int id)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                var lopHoc = await _context.LopHocs
                    .Include(l => l.GiangVien)
                    .ThenInclude(gv => gv.NguoiDung)
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                if (lopHoc.GiangVien == null || lopHoc.GiangVien.NguoiDung == null)
                {
                    return Ok(new TruongKhoaApiResponse<ThongTinGiangVienDTO>
                    {
                        Success = true,
                        Message = "Lớp học chưa được phân công giảng viên",
                        Data = null
                    });
                }

                var giangVienDTO = new ThongTinGiangVienDTO
                {
                    GiangVienId = lopHoc.GiangVien.GiangVienId,
                    HoTen = lopHoc.GiangVien.NguoiDung.HoTen,
                    Email = lopHoc.GiangVien.NguoiDung.Email,
                    HocVi = lopHoc.GiangVien.HocVi,
                    ChuyenMon = lopHoc.GiangVien.ChuyenMon
                };

                return Ok(new TruongKhoaApiResponse<ThongTinGiangVienDTO>
                {
                    Success = true,
                    Data = giangVienDTO
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin giảng viên lớp ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 7. PHÂN CÔNG GIẢNG VIÊN
        [HttpPost("{id}/phan-cong-giang-vien")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> PhanCongGiangVien(int id, [FromBody] PhanCongGiangVienRequestDTO request)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                var giangVien = await _context.GiangViens
                    .Include(gv => gv.NguoiDung)
                    .FirstOrDefaultAsync(gv => gv.GiangVienId == request.GiangVienId);

                if (giangVien == null || giangVien.NguoiDung == null)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Giảng viên không tồn tại" });
                }

                // Kiểm tra giảng viên có thuộc khoa không
                if (giangVien.KhoaId != khoaIdTruongKhoa.Value)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Giảng viên không thuộc khoa của bạn" });
                }

                // Kiểm tra trùng lịch
                var coTrungLich = await KiemTraTrungLichGiangVien(request.GiangVienId, id);
                if (coTrungLich)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Giảng viên bị trùng lịch dạy" });
                }

                // Cập nhật giảng viên cho lớp
                lopHoc.GiangVienId = request.GiangVienId;
                lopHoc.UpdatedAt = DateTime.Now;

                // Thêm vào bảng phân công giảng viên - lớp
                var giangVienLop = new GiangVienLop
                {
                    GiangVienId = request.GiangVienId,
                    LopHocId = id,
                    TenGiangVien = giangVien.NguoiDung.HoTen,
                    NgayPhanCong = request.NgayBatDauPhanCong ?? DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                _context.GiangVienLops.Add(giangVienLop);
                _context.LopHocs.Update(lopHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Phân công giảng viên thành công",
                    Data = new
                    {
                        lopHocId = lopHoc.LopHocId,
                        maLop = lopHoc.MaLop,
                        giangVienMoi = giangVien.NguoiDung.HoTen,
                        emailGiangVien = giangVien.NguoiDung.Email,
                        ngayPhanCong = giangVienLop.NgayPhanCong
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi phân công giảng viên lớp ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 8. KIỂM TRA TRÙNG LỊCH GIẢNG VIÊN
        [HttpGet("{id}/kiem-tra-trung-lich")]
        public async Task<ActionResult<TruongKhoaApiResponse<KiemTraTrungLichResponseDTO>>> KiemTraTrungLich(int id, [FromQuery] int giangVienId)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Lấy lịch dạy của lớp hiện tại
                var lopHoc = await _context.LopHocs
                    .Include(l => l.BuoiHocs)
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                // Lấy tất cả lớp học mà giảng viên đang dạy (trừ lớp hiện tại nếu đang thay đổi)
                var lopHocCuaGiangVien = await _context.LopHocs
                    .Include(l => l.BuoiHocs)
                    .ThenInclude(bh => bh.PhongHoc)
                    .Where(l => l.GiangVienId == giangVienId && l.LopHocId != id && l.TrangThai == 1)
                    .ToListAsync();

                var chiTietTrungLich = new List<ChiTietTrungLichDTO>();
                var coTrungLich = false;

                // So sánh lịch của lớp hiện tại với các lớp của giảng viên
                foreach (var buoiHocLopHienTai in lopHoc.BuoiHocs)
                {
                    foreach (var lopKhac in lopHocCuaGiangVien)
                    {
                        foreach (var buoiHocLopKhac in lopKhac.BuoiHocs)
                        {
                            // Kiểm tra trùng thứ và trùng giờ
                            if (buoiHocLopHienTai.Thứ == buoiHocLopKhac.Thứ &&
                                ((buoiHocLopHienTai.ThoiGianBatDau >= buoiHocLopKhac.ThoiGianBatDau && buoiHocLopHienTai.ThoiGianBatDau < buoiHocLopKhac.ThoiGianKetThuc) ||
                                 (buoiHocLopHienTai.ThoiGianKetThuc > buoiHocLopKhac.ThoiGianBatDau && buoiHocLopHienTai.ThoiGianKetThuc <= buoiHocLopKhac.ThoiGianKetThuc) ||
                                 (buoiHocLopHienTai.ThoiGianBatDau <= buoiHocLopKhac.ThoiGianBatDau && buoiHocLopHienTai.ThoiGianKetThuc >= buoiHocLopKhac.ThoiGianKetThuc)))
                            {
                                coTrungLich = true;
                                chiTietTrungLich.Add(new ChiTietTrungLichDTO
                                {
                                    Thu = buoiHocLopHienTai.Thứ,
                                    GioBatDau = buoiHocLopHienTai.ThoiGianBatDau.TimeOfDay,
                                    GioKetThuc = buoiHocLopHienTai.ThoiGianKetThuc.TimeOfDay,
                                    Phong = buoiHocLopKhac.PhongHoc != null ? buoiHocLopKhac.PhongHoc.MaPhong : null,
                                    CoSo = buoiHocLopKhac.PhongHoc != null ? buoiHocLopKhac.PhongHoc.DiaChi : null,
                                    LopHocTrung = $"{lopKhac.MaLop} - {lopKhac.TenLop}"
                                });
                            }
                        }
                    }
                }

                var response = new KiemTraTrungLichResponseDTO
                {
                    CoTrungLich = coTrungLich,
                    ChiTietTrungLich = chiTietTrungLich
                };

                return Ok(new TruongKhoaApiResponse<KiemTraTrungLichResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi kiểm tra trùng lịch lớp ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 9. DANH SÁCH SINH VIÊN TRONG LỚP
        [HttpGet("{id}/sinh-vien")]
        public async Task<ActionResult<TruongKhoaPagedResponse<ThongTinSinhVienDTO>>> GetDanhSachSinhVien(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? nganhId = null)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaPagedResponse<ThongTinSinhVienDTO>
                    {
                        Success = false,
                        Data = new TruongKhoaPagedData<ThongTinSinhVienDTO>
                        {
                            Items = new List<ThongTinSinhVienDTO>()
                        }
                    });
                }

                // Kiểm tra lớp có thuộc khoa của Trưởng khoa
                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaPagedResponse<ThongTinSinhVienDTO>
                    {
                        Success = false,
                        Data = new TruongKhoaPagedData<ThongTinSinhVienDTO>
                        {
                            Items = new List<ThongTinSinhVienDTO>()
                        }
                    });
                }

                var query = _context.SinhVienLops
                    .Include(sl => sl.SinhVien)
                    .ThenInclude(sv => sv.NguoiDung)
                    .Include(sl => sl.SinhVien)
                    .ThenInclude(sv => sv.Nganh)
                    .Include(sl => sl.SinhVien)
                    .ThenInclude(sv => sv.KhoaTuyenSinh)
                    .Where(sl => sl.LopHocId == id && sl.TinhTrang == "dang hoc");

                // Áp dụng bộ lọc
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(sl =>
                        (sl.SinhVien.Mssv != null && sl.SinhVien.Mssv.Contains(search)) ||
                        (sl.SinhVien.NguoiDung != null && sl.SinhVien.NguoiDung.HoTen != null &&
                         sl.SinhVien.NguoiDung.HoTen.Contains(search)));
                }

                if (nganhId.HasValue)
                {
                    query = query.Where(sl => sl.SinhVien.NganhId == nganhId.Value);
                }

                // Phân trang
                var total = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(total / (double)limit);

                var sinhViens = await query
                    .OrderBy(sl => sl.SinhVien.NguoiDung != null ? sl.SinhVien.NguoiDung.HoTen : "")
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(sl => new ThongTinSinhVienDTO
                    {
                        SinhVienId = sl.SinhVienId,
                        MSSV = sl.SinhVien.Mssv,
                        HoTen = sl.SinhVien.NguoiDung != null ? sl.SinhVien.NguoiDung.HoTen : null,
                        Email = sl.SinhVien.NguoiDung != null ? sl.SinhVien.NguoiDung.Email : null,
                        TenNganh = sl.SinhVien.Nganh != null ? sl.SinhVien.Nganh.TenNganh : null,
                        TenKhoaTuyenSinh = sl.SinhVien.KhoaTuyenSinh != null ? sl.SinhVien.KhoaTuyenSinh.TenKhoaTuyenSinh : null,
                        GPA = sl.SinhVien.Gpa,
                        TrangThai = sl.TinhTrang,
                        NgayVaoLop = sl.NgayVao
                    })
                    .ToListAsync();

                var response = new TruongKhoaPagedResponse<ThongTinSinhVienDTO>
                {
                    Data = new TruongKhoaPagedData<ThongTinSinhVienDTO>
                    {
                        Items = sinhViens,
                        Total = total,
                        Page = page,
                        Limit = limit,
                        TotalPages = totalPages
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách sinh viên lớp ID: {id}");
                return StatusCode(500, new TruongKhoaPagedResponse<ThongTinSinhVienDTO>
                {
                    Success = false,
                    Data = new TruongKhoaPagedData<ThongTinSinhVienDTO>
                    {
                        Items = new List<ThongTinSinhVienDTO>()
                    }
                });
            }
        }

        // 10. THÊM SINH VIÊN VÀO LỚP
        [HttpPost("{id}/sinh-vien")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> ThemSinhVienVaoLop(int id, [FromBody] ThemSinhVienVaoLopRequestDTO request)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Kiểm tra lớp có thuộc khoa của Trưởng khoa
                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                // Kiểm tra sĩ số
                if (lopHoc.SiSo >= lopHoc.SiSoToiDa)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Lớp đã đầy sĩ số" });
                }

                // Kiểm tra sinh viên có tồn tại không
                var sinhVien = await _context.HoSoSinhViens
                    .Include(sv => sv.NguoiDung)
                    .FirstOrDefaultAsync(sv => sv.SinhVienId == request.SinhVienId);

                if (sinhVien == null)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Sinh viên không tồn tại" });
                }

                // Kiểm tra sinh viên đã có trong lớp chưa
                var daCoTrongLop = await _context.SinhVienLops
                    .AnyAsync(sl => sl.SinhVienId == request.SinhVienId && sl.LopHocId == id && sl.TinhTrang == "dang hoc");

                if (daCoTrongLop)
                {
                    return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Sinh viên đã có trong lớp" });
                }

                // Thêm sinh viên vào lớp
                var sinhVienLop = new SinhVienLop
                {
                    SinhVienId = request.SinhVienId,
                    LopHocId = id,
                    TinhTrang = request.TinhTrang ?? "dang hoc",
                    NgayVao = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                // Cập nhật sĩ số lớp
                lopHoc.SiSo = (lopHoc.SiSo ?? 0) + 1;
                lopHoc.UpdatedAt = DateTime.Now;

                _context.SinhVienLops.Add(sinhVienLop);
                _context.LopHocs.Update(lopHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Thêm sinh viên vào lớp thành công",
                    Data = new
                    {
                        sinhVienId = sinhVien.SinhVienId,
                        mssv = sinhVien.Mssv,
                        hoTen = sinhVien.NguoiDung != null ? sinhVien.NguoiDung.HoTen : null,
                        ngayVaoLop = sinhVienLop.NgayVao
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi thêm sinh viên vào lớp ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 11. XÓA SINH VIÊN KHỎI LỚP
        [HttpDelete("{id}/sinh-vien/{svId}")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> XoaSinhVienKhoiLop(int id, int svId)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Kiểm tra lớp có thuộc khoa của Trưởng khoa
                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                // Tìm sinh viên trong lớp
                var sinhVienLop = await _context.SinhVienLops
                    .FirstOrDefaultAsync(sl => sl.SinhVienId == svId && sl.LopHocId == id && sl.TinhTrang == "dang hoc");

                if (sinhVienLop == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy sinh viên trong lớp" });
                }

                // Cập nhật trạng thái (xóa mềm)
                sinhVienLop.TinhTrang = "da nghi";
                sinhVienLop.UpdatedAt = DateTime.Now;

                // Cập nhật sĩ số lớp
                lopHoc.SiSo = Math.Max((lopHoc.SiSo ?? 1) - 1, 0);
                lopHoc.UpdatedAt = DateTime.Now;

                _context.SinhVienLops.Update(sinhVienLop);
                _context.LopHocs.Update(lopHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa sinh viên khỏi lớp thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa sinh viên khỏi lớp ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 12. LỊCH DẠY CỦA LỚP
        [HttpGet("{id}/lich-day")]
        public async Task<ActionResult<TruongKhoaApiResponse<List<LichDayDTO>>>> GetLichDayCuaLop(int id)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Kiểm tra lớp có thuộc khoa của Trưởng khoa
                var lopHoc = await _context.LopHocs
                    .Include(l => l.BuoiHocs)
                    .ThenInclude(bh => bh.PhongHoc)
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                var lichDay = lopHoc.BuoiHocs
                    .Select(bh => new LichDayDTO
                    {
                        BuoiHocId = bh.BuoiHocId,
                        Thu = bh.Thứ,
                        GioBatDau = bh.ThoiGianBatDau.TimeOfDay,
                        GioKetThuc = bh.ThoiGianKetThuc.TimeOfDay,
                        TenPhong = bh.PhongHoc != null ? bh.PhongHoc.TenPhong : null,
                        MaPhong = bh.PhongHoc != null ? bh.PhongHoc.MaPhong : null,
                        DiaChi = bh.PhongHoc != null ? bh.PhongHoc.DiaChi : null,
                        LoaiBuoiHoc = bh.LoaiBuoiHoc,
                        SoBuoi = bh.SoBuoi ?? 1,
                        GhiChu = bh.GhiChu
                    })
                    .ToList();

                return Ok(new TruongKhoaApiResponse<List<LichDayDTO>>
                {
                    Success = true,
                    Data = lichDay
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy lịch dạy lớp ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 13. THÊM LỊCH DẠY
        [HttpPost("{id}/lich-day")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> ThemLichDay(int id, [FromBody] TaoLichDayRequestDTO request)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Kiểm tra lớp có thuộc khoa của Trưởng khoa
                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                // Kiểm tra phòng học (nếu có)
                if (request.PhongHocId.HasValue)
                {
                    var phongHoc = await _context.PhongHocs.FindAsync(request.PhongHocId.Value);
                    if (phongHoc == null)
                    {
                        return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Phòng học không tồn tại" });
                    }
                }

                // Kiểm tra trùng lịch (nếu có giảng viên)
                if (lopHoc.GiangVienId.HasValue)
                {
                    var coTrungLich = await KiemTraTrungLichGiangVien(lopHoc.GiangVienId.Value, id, request);
                    if (coTrungLich)
                    {
                        return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Lịch dạy bị trùng với giảng viên hiện tại" });
                    }
                }

                // Tạo buổi học
                var buoiHoc = new BuoiHoc
                {
                    LopHocId = id,
                    PhongHocId = request.PhongHocId,
                    Thứ = request.Thu ?? "",
                    ThoiGianBatDau = (lopHoc.NgayBatDau ?? DateOnly.FromDateTime(DateTime.Now))
                        .ToDateTime(new TimeOnly(request.GioBatDau.Hours, request.GioBatDau.Minutes)),
                    ThoiGianKetThuc = (lopHoc.NgayBatDau ?? DateOnly.FromDateTime(DateTime.Now))
                        .ToDateTime(new TimeOnly(request.GioKetThuc.Hours, request.GioKetThuc.Minutes)),
                    LoaiBuoiHoc = request.LoaiBuoiHoc ?? "giang bai",
                    SoBuoi = request.SoBuoi,
                    GhiChu = request.GhiChu,
                    CreatedAt = DateTime.Now
                };

                _context.BuoiHocs.Add(buoiHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Thêm lịch dạy thành công",
                    Data = new
                    {
                        buoiHocId = buoiHoc.BuoiHocId,
                        thu = buoiHoc.Thứ,
                        gioBatDau = buoiHoc.ThoiGianBatDau.TimeOfDay,
                        gioKetThuc = buoiHoc.ThoiGianKetThuc.TimeOfDay
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi thêm lịch dạy lớp ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 14. CẬP NHẬT LỊCH DẠY
        [HttpPut("{id}/lich-day/{lichId}")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> CapNhatLichDay(int id, int lichId, [FromBody] CapNhatLichDayRequestDTO request)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Kiểm tra lớp có thuộc khoa của Trưởng khoa
                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                // Tìm buổi học
                var buoiHoc = await _context.BuoiHocs
                    .FirstOrDefaultAsync(bh => bh.BuoiHocId == lichId && bh.LopHocId == id);

                if (buoiHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lịch dạy" });
                }

                // Kiểm tra phòng học (nếu có)
                if (request.PhongHocId.HasValue)
                {
                    var phongHoc = await _context.PhongHocs.FindAsync(request.PhongHocId.Value);
                    if (phongHoc == null)
                    {
                        return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Phòng học không tồn tại" });
                    }
                    buoiHoc.PhongHocId = request.PhongHocId.Value;
                }

                // Cập nhật thông tin
                if (!string.IsNullOrEmpty(request.Thu))
                    buoiHoc.Thứ = request.Thu;

                if (request.GioBatDau.HasValue && request.GioKetThuc.HasValue)
                {
                    // Giữ nguyên ngày, chỉ cập nhật giờ
                    var ngay = buoiHoc.ThoiGianBatDau.Date;
                    buoiHoc.ThoiGianBatDau = ngay + request.GioBatDau.Value;
                    buoiHoc.ThoiGianKetThuc = ngay + request.GioKetThuc.Value;
                }

                if (!string.IsNullOrEmpty(request.LoaiBuoiHoc))
                    buoiHoc.LoaiBuoiHoc = request.LoaiBuoiHoc;

                if (request.SoBuoi.HasValue)
                    buoiHoc.SoBuoi = request.SoBuoi.Value;

                if (!string.IsNullOrEmpty(request.GhiChu))
                    buoiHoc.GhiChu = request.GhiChu;

                buoiHoc.UpdatedAt = DateTime.Now;

                // Kiểm tra trùng lịch (nếu có giảng viên và thay đổi giờ/thứ)
                if (lopHoc.GiangVienId.HasValue &&
                    (!string.IsNullOrEmpty(request.Thu) || request.GioBatDau.HasValue))
                {
                    var coTrungLich = await KiemTraTrungLichGiangVien(lopHoc.GiangVienId.Value, id, buoiHoc);
                    if (coTrungLich)
                    {
                        return BadRequest(new TruongKhoaApiResponse<object> { Success = false, Message = "Lịch dạy bị trùng với giảng viên hiện tại" });
                    }
                }

                _context.BuoiHocs.Update(buoiHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Cập nhật lịch dạy thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật lịch dạy ID: {lichId}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 15. XÓA LỊCH DẠY
        [HttpDelete("{id}/lich-day/{lichId}")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> XoaLichDay(int id, int lichId)
        {
            try
            {
                // Lấy khoa của Trưởng khoa
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Kiểm tra lớp có thuộc khoa của Trưởng khoa
                var lopHoc = await _context.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id && l.KhoaId == khoaIdTruongKhoa.Value);

                if (lopHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lớp học" });
                }

                // Tìm buổi học
                var buoiHoc = await _context.BuoiHocs
                    .FirstOrDefaultAsync(bh => bh.BuoiHocId == lichId && bh.LopHocId == id);

                if (buoiHoc == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy lịch dạy" });
                }

                _context.BuoiHocs.Remove(buoiHoc);
                await _context.SaveChangesAsync();

                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa lịch dạy thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa lịch dạy ID: {lichId}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 16. TÌM KIẾM LỚP HỌC
        [HttpGet("tim-kiem")]
        public async Task<ActionResult<TruongKhoaPagedResponse<LopHocListDTO>>> TimKiemLopHoc(
            [FromQuery] TimKiemLopHocDTO timKiemDTO)
        {
            return await GetDanhSachLopHoc(
                timKiemDTO.Page,
                timKiemDTO.Limit,
                timKiemDTO.Search,
                timKiemDTO.MonHocId,
                timKiemDTO.KhoaId,
                timKiemDTO.NganhId,
                timKiemDTO.GiangVienId,
                timKiemDTO.TrangThai);
        }

        // 17. LỌC LỚP HỌC
        [HttpGet("filter")]
        public async Task<ActionResult<TruongKhoaPagedResponse<LopHocListDTO>>> FilterLopHoc(
            [FromQuery] TimKiemLopHocDTO filterDTO)
        {
            return await GetDanhSachLopHoc(
                filterDTO.Page,
                filterDTO.Limit,
                filterDTO.Search,
                filterDTO.MonHocId,
                filterDTO.KhoaId,
                filterDTO.NganhId,
                filterDTO.GiangVienId,
                filterDTO.TrangThai);
        }

        // ============ PHƯƠNG THỨC HỖ TRỢ ============

        // Phương thức kiểm tra trùng lịch giảng viên
        private async Task<bool> KiemTraTrungLichGiangVien(int giangVienId, int lopHocId, TaoLichDayRequestDTO? newLich = null)
        {
            // Lấy tất cả lớp học mà giảng viên đang dạy
            var lopHocCuaGiangVien = await _context.LopHocs
                .Include(l => l.BuoiHocs)
                .Where(l => l.GiangVienId == giangVienId && l.LopHocId != lopHocId && l.TrangThai == 1)
                .ToListAsync();

            // Lấy lịch của lớp hiện tại
            var buoiHocsLopHienTai = await _context.BuoiHocs
                .Where(bh => bh.LopHocId == lopHocId)
                .ToListAsync();

            // Nếu có lịch mới thêm vào, kiểm tra với nó
            if (newLich != null)
            {
                foreach (var lopKhac in lopHocCuaGiangVien)
                {
                    foreach (var buoiHocLopKhac in lopKhac.BuoiHocs)
                    {
                        // Kiểm tra trùng thứ và trùng giờ
                        if (newLich.Thu == buoiHocLopKhac.Thứ &&
                            ((newLich.GioBatDau >= buoiHocLopKhac.ThoiGianBatDau.TimeOfDay && newLich.GioBatDau < buoiHocLopKhac.ThoiGianKetThuc.TimeOfDay) ||
                             (newLich.GioKetThuc > buoiHocLopKhac.ThoiGianBatDau.TimeOfDay && newLich.GioKetThuc <= buoiHocLopKhac.ThoiGianKetThuc.TimeOfDay) ||
                             (newLich.GioBatDau <= buoiHocLopKhac.ThoiGianBatDau.TimeOfDay && newLich.GioKetThuc >= buoiHocLopKhac.ThoiGianKetThuc.TimeOfDay)))
                        {
                            return true;
                        }
                    }
                }
            }

            // Kiểm tra với các lịch hiện có của lớp
            foreach (var buoiHocLopHienTai in buoiHocsLopHienTai)
            {
                foreach (var lopKhac in lopHocCuaGiangVien)
                {
                    foreach (var buoiHocLopKhac in lopKhac.BuoiHocs)
                    {
                        // Kiểm tra trùng thứ và trùng giờ
                        if (buoiHocLopHienTai.Thứ == buoiHocLopKhac.Thứ &&
                            ((buoiHocLopHienTai.ThoiGianBatDau >= buoiHocLopKhac.ThoiGianBatDau && buoiHocLopHienTai.ThoiGianBatDau < buoiHocLopKhac.ThoiGianKetThuc) ||
                             (buoiHocLopHienTai.ThoiGianKetThuc > buoiHocLopKhac.ThoiGianBatDau && buoiHocLopHienTai.ThoiGianKetThuc <= buoiHocLopKhac.ThoiGianKetThuc) ||
                             (buoiHocLopHienTai.ThoiGianBatDau <= buoiHocLopKhac.ThoiGianBatDau && buoiHocLopHienTai.ThoiGianKetThuc >= buoiHocLopKhac.ThoiGianKetThuc)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // Overload cho kiểm tra với BuoiHoc
        private async Task<bool> KiemTraTrungLichGiangVien(int giangVienId, int lopHocId, BuoiHoc buoiHoc)
        {
            // Tạo DTO từ BuoiHoc
            var lichDay = new TaoLichDayRequestDTO
            {
                Thu = buoiHoc.Thứ,
                GioBatDau = buoiHoc.ThoiGianBatDau.TimeOfDay,
                GioKetThuc = buoiHoc.ThoiGianKetThuc.TimeOfDay,
                PhongHocId = buoiHoc.PhongHocId
            };

            return await KiemTraTrungLichGiangVien(giangVienId, lopHocId, lichDay);
        }
    }
}