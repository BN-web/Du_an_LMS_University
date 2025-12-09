using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LMS_GV.Models;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/lop-hoc")]
    [Authorize(Roles = "Admin")]
    public class LopHocController : ControllerBase
    {
            private readonly AppDbContext _db;

            public LopHocController(AppDbContext db)
            {
                _db = db;
            }

            public class LopHocListQuery
            {
                public string? Search { get; set; }
                public int? KhoaId { get; set; }
                public int? NganhId { get; set; }
                public byte? Status { get; set; } // 1=đang học, 2=đã kết thúc
            }

            public class CreateLopHocRequest
            {
                [Required]
                [StringLength(10, ErrorMessage = "Mã lớp tối đa 10 ký tự")]
                public string MaLop { get; set; } = string.Empty;

                [Required]
                public string TenLop { get; set; } = string.Empty;

                [Required]
                public int MonHocId { get; set; }

                [Required]
                public int HocKyId { get; set; }

                [Required]
                public int KhoaId { get; set; }

                [Required]
                public int NganhId { get; set; }

                [Required]
                public int SoTinChi { get; set; }

                // FE gửi DateTime, server convert sang DateOnly để lưu
                public DateTime? NgayBatDau { get; set; }
                public DateTime? NgayKetThuc { get; set; }

                public int? SiSoToiDa { get; set; }
                public int? AbsentAllowed { get; set; } // SoBuoiVangChoPhep
                public int? Periods { get; set; }        // SoTiet

                public byte? Status { get; set; }        // 1/2, mặc định 1
            }

            public class UpdateLopHocRequest
            {
                [Required]
                [StringLength(10, ErrorMessage = "Mã lớp tối đa 10 ký tự")]
                public string MaLop { get; set; } = string.Empty;

                [Required]
                public string TenLop { get; set; } = string.Empty;

                [Required]
                public int MonHocId { get; set; }

                [Required]
                public int HocKyId { get; set; }

                [Required]
                public int KhoaId { get; set; }

                [Required]
                public int NganhId { get; set; }

                [Required]
                public int SoTinChi { get; set; }

                public DateTime? NgayBatDau { get; set; }
                public DateTime? NgayKetThuc { get; set; }

                public int? SiSoToiDa { get; set; }
                public int? AbsentAllowed { get; set; }
                public int? Periods { get; set; }

                public byte? Status { get; set; }
            }

            public class UpdateLopHocStatusRequest
            {
                [Required]
                public byte Status { get; set; } // 1=đang học, 2=đã kết thúc
            }

            // ========== 1. GET / – list ==========

            [HttpGet]
            public async Task<IActionResult> GetList([FromQuery] LopHocListQuery queryModel)
            {
                var query = _db.LopHocs.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(queryModel.Search))
                {
                    var kw = queryModel.Search.Trim();
                    query = query.Where(l =>
                        (l.MaLop ?? "").Contains(kw) ||
                        (l.TenLop ?? "").Contains(kw));
                }

                if (queryModel.KhoaId.HasValue)
                {
                    var kId = queryModel.KhoaId.Value;
                    query = query.Where(l => l.KhoaId == kId);
                }

                if (queryModel.NganhId.HasValue)
                {
                    var nId = queryModel.NganhId.Value;
                    query = query.Where(l => l.NganhId == nId);
                }

                if (queryModel.Status.HasValue)
                {
                    var st = queryModel.Status.Value;
                    query = query.Where(l => l.TrangThai == st);
                }

                var list = await query
                    .OrderBy(l => l.MaLop)
                    .Select(l => new
                    {
                        id = l.LopHocId,
                        code = l.MaLop,
                        name = l.TenLop,
                        soTinChi = l.SoTinChi,
                        status = l.TrangThai,
                        khoaId = l.KhoaId,
                        khoaName = _db.Khoas
                            .Where(k => k.KhoaId == l.KhoaId)
                            .Select(k => k.TenKhoa)
                            .FirstOrDefault(),
                        nganhId = l.NganhId,
                        nganhName = _db.Nganhs
                            .Where(n => n.NganhId == l.NganhId)
                            .Select(n => n.TenNganh)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(list);
            }

            // ========== 2. GET /{id} – detail ==========

            [HttpGet("{id:int}")]
            public async Task<IActionResult> GetDetail(int id)
            {
                var lop = await _db.LopHocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.LopHocId == id);

                if (lop == null)
                    return NotFound(new { message = "Không tìm thấy lớp học" });

                var khoa = await _db.Khoas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(k => k.KhoaId == lop.KhoaId);

                var nganh = await _db.Nganhs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.NganhId == lop.NganhId);

                var mon = await _db.MonHocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MonHocId == lop.MonHocId);

                var hocKy = await _db.HocKys
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.HocKyId == lop.HocKyId);

                // Giảng viên
                var giangVienInfo = await (
                    from gv in _db.GiangViens
                    join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                    where gv.GiangVienId == lop.GiangVienId
                    select new
                    {
                        name = nd.HoTen,
                        email = nd.Email
                    }
                ).FirstOrDefaultAsync();

                var totalStudents = await _db.SinhVienLops
                    .CountAsync(sl => sl.LopHocId == id);

                var result = new
                {
                    id = lop.LopHocId,
                    code = lop.MaLop,
                    name = lop.TenLop,
                    monHocId = lop.MonHocId,
                    monHocName = mon?.TenMon,
                    soTinChi = lop.SoTinChi,
                    khoaId = lop.KhoaId,
                    khoaName = khoa?.TenKhoa,
                    nganhId = lop.NganhId,
                    nganhName = nganh?.TenNganh,
                    hocKyId = lop.HocKyId,
                    hocKy = hocKy?.KiHoc,
                    namHoc = hocKy?.NamHoc,
                    giangVien = giangVienInfo,
                    totalStudents,
                    status = lop.TrangThai,
                    // nếu property trong entity là DateOnly? thì vẫn trả ra được,
                    // JSON sẽ là "2025-01-01"
                    ngayBatDau = lop.NgayBatDau,
                    ngayKetThuc = lop.NgayKetThuc,
                    soBuoiVangChoPhep = lop.SoBuoiVangChoPhep,
                    soTiet = lop.SoTiet,
                    siSo = lop.SiSo,
                    siSoToiDa = lop.SiSoToiDa,
                    createdAt = lop.CreatedAt,
                    updatedAt = lop.UpdatedAt
                };

                return Ok(result);
            }

            // ========== 3. POST / – create ==========

            [HttpPost]
            public async Task<IActionResult> Create([FromBody] CreateLopHocRequest req)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (req.MaLop.Length > 10)
                    return BadRequest(new { message = "Mã lớp tối đa 10 ký tự" });

                var existsCode = await _db.LopHocs
                    .AnyAsync(l => l.MaLop == req.MaLop);
                if (existsCode)
                {
                    return Conflict(new
                    {
                        field = "maLop",
                        message = "Mã lớp đã tồn tại"
                    });
                }

                // Check tồn tại các FK
                var existsMon = await _db.MonHocs.AnyAsync(m => m.MonHocId == req.MonHocId);
                if (!existsMon)
                    return BadRequest(new { field = "monHocId", message = "Môn học không tồn tại" });

                var existsKhoa = await _db.Khoas.AnyAsync(k => k.KhoaId == req.KhoaId);
                if (!existsKhoa)
                    return BadRequest(new { field = "khoaId", message = "Khoa không tồn tại" });

                var existsNganh = await _db.Nganhs.AnyAsync(n => n.NganhId == req.NganhId);
                if (!existsNganh)
                    return BadRequest(new { field = "nganhId", message = "Ngành không tồn tại" });

                var existsHocKy = await _db.HocKys.AnyAsync(h => h.HocKyId == req.HocKyId);
                if (!existsHocKy)
                    return BadRequest(new { field = "hocKyId", message = "Học kỳ không tồn tại" });

                var lop = new LopHoc
                {
                    MaLop = req.MaLop,
                    TenLop = req.TenLop,
                    MonHocId = req.MonHocId,
                    HocKyId = req.HocKyId,
                    KhoaId = req.KhoaId,
                    NganhId = req.NganhId,
                    SoTinChi = req.SoTinChi,

                    // ✅ convert DateTime? -> DateOnly?
                    NgayBatDau = req.NgayBatDau.HasValue
                        ? DateOnly.FromDateTime(req.NgayBatDau.Value)
                        : (DateOnly?)null,
                    NgayKetThuc = req.NgayKetThuc.HasValue
                        ? DateOnly.FromDateTime(req.NgayKetThuc.Value)
                        : (DateOnly?)null,

                    SiSo = 0,
                    SiSoToiDa = req.SiSoToiDa,
                    SoBuoiVangChoPhep = req.AbsentAllowed,
                    SoTiet = req.Periods,
                    TrangThai = req.Status ?? 1,
                    CreatedAt = DateTime.UtcNow
                };

                _db.LopHocs.Add(lop);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDetail), new { id = lop.LopHocId }, new
                {
                    id = lop.LopHocId,
                    code = lop.MaLop
                });
            }

            // ========== 4. PUT /{id} – update ==========

            [HttpPut("{id:int}")]
            public async Task<IActionResult> Update(int id, [FromBody] UpdateLopHocRequest req)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var lop = await _db.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id);
                if (lop == null)
                    return NotFound(new { message = "Không tìm thấy lớp học" });

                if (req.MaLop.Length > 10)
                    return BadRequest(new { message = "Mã lớp tối đa 10 ký tự" });

                var existsCode = await _db.LopHocs
                    .AnyAsync(l => l.MaLop == req.MaLop && l.LopHocId != id);
                if (existsCode)
                {
                    return Conflict(new
                    {
                        field = "maLop",
                        message = "Mã lớp đã tồn tại"
                    });
                }

                // Check FK
                var existsMon = await _db.MonHocs.AnyAsync(m => m.MonHocId == req.MonHocId);
                if (!existsMon)
                    return BadRequest(new { field = "monHocId", message = "Môn học không tồn tại" });

                var existsKhoa = await _db.Khoas.AnyAsync(k => k.KhoaId == req.KhoaId);
                if (!existsKhoa)
                    return BadRequest(new { field = "khoaId", message = "Khoa không tồn tại" });

                var existsNganh = await _db.Nganhs.AnyAsync(n => n.NganhId == req.NganhId);
                if (!existsNganh)
                    return BadRequest(new { field = "nganhId", message = "Ngành không tồn tại" });

                var existsHocKy = await _db.HocKys.AnyAsync(h => h.HocKyId == req.HocKyId);
                if (!existsHocKy)
                    return BadRequest(new { field = "hocKyId", message = "Học kỳ không tồn tại" });

                lop.MaLop = req.MaLop;
                lop.TenLop = req.TenLop;
                lop.MonHocId = req.MonHocId;
                lop.HocKyId = req.HocKyId;
                lop.KhoaId = req.KhoaId;
                lop.NganhId = req.NganhId;
                lop.SoTinChi = req.SoTinChi;

                // ✅ convert DateTime? -> DateOnly?
                lop.NgayBatDau = req.NgayBatDau.HasValue
                    ? DateOnly.FromDateTime(req.NgayBatDau.Value)
                    : (DateOnly?)null;
                lop.NgayKetThuc = req.NgayKetThuc.HasValue
                    ? DateOnly.FromDateTime(req.NgayKetThuc.Value)
                    : (DateOnly?)null;

                lop.SiSoToiDa = req.SiSoToiDa;
                lop.SoBuoiVangChoPhep = req.AbsentAllowed;
                lop.SoTiet = req.Periods;

                if (req.Status.HasValue)
                    lop.TrangThai = req.Status.Value;

                lop.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return NoContent();
            }

            // ========== 5. PUT /{id}/status – change status ==========

            [HttpPut("{id:int}/status")]
            public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateLopHocStatusRequest req)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var lop = await _db.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id);
                if (lop == null)
                    return NotFound(new { message = "Không tìm thấy lớp học" });

                lop.TrangThai = req.Status;
                lop.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return NoContent();
            }

            // ========== 6. DELETE /{id} – delete ==========

            [HttpDelete("{id:int}")]
            public async Task<IActionResult> Delete(int id)
            {
                var lop = await _db.LopHocs
                    .FirstOrDefaultAsync(l => l.LopHocId == id);
                if (lop == null)
                    return NotFound(new { message = "Không tìm thấy lớp học" });

                var usedByStudents = await _db.SinhVienLops
                    .AnyAsync(sl => sl.LopHocId == id);
                var hasBuoiHoc = await _db.BuoiHocs
                    .AnyAsync(bh => bh.LopHocId == id);
                var hasBuoiThi = await _db.BuoiThis
                    .AnyAsync(bt => bt.LopHocId == id);

                if (usedByStudents || hasBuoiHoc || hasBuoiThi)
                {
                    return BadRequest(new
                    {
                        message = "Không thể xóa lớp vì đang có sinh viên/buổi học/buổi thi liên quan"
                    });
                }

                _db.LopHocs.Remove(lop);
                await _db.SaveChangesAsync();
                return NoContent();
            }

            // ========== 7. GET /{id}/buoi-hoc – lịch học ==========

            [HttpGet("{id:int}/buoi-hoc")]
            public async Task<IActionResult> GetBuoiHoc(int id)
            {
                var exists = await _db.LopHocs
                    .AnyAsync(l => l.LopHocId == id);
                if (!exists)
                    return NotFound(new { message = "Không tìm thấy lớp học" });

                var data = await (
                    from bh in _db.BuoiHocs
                    join ph in _db.PhongHocs on bh.PhongHocId equals ph.PhongHocId into gph
                    from ph in gph.DefaultIfEmpty()
                    where bh.LopHocId == id
                    select new
                    {
                        buoiHocId = bh.BuoiHocId,
                        thu = bh.Thứ,   
                        thoiGianBatDau = bh.ThoiGianBatDau,
                        thoiGianKetThuc = bh.ThoiGianKetThuc,
                        loaiBuoiHoc = bh.LoaiBuoiHoc,
                        trangThai = bh.TrangThai,
                        soBuoi = bh.SoBuoi,
                        phongHoc = ph != null ? ph.TenPhong : null,
                        maPhong = ph != null ? ph.MaPhong : null,
                        diaChi = ph != null ? ph.DiaChi : null
                    }
                ).ToListAsync();

                return Ok(data);
            }

            // ========== 8. GET /{id}/buoi-thi – lịch thi ==========

            [HttpGet("{id:int}/buoi-thi")]
            public async Task<IActionResult> GetBuoiThi(int id)
            {
                var exists = await _db.LopHocs
                    .AnyAsync(l => l.LopHocId == id);
                if (!exists)
                    return NotFound(new { message = "Không tìm thấy lớp học" });

                var data = await (
                    from bt in _db.BuoiThis
                    join ph in _db.PhongHocs on bt.PhongHocId equals ph.PhongHocId into gph
                    from ph in gph.DefaultIfEmpty()
                    where bt.LopHocId == id
                    select new
                    {
                        buoiThiId = bt.BuoiThiId,
                        ngayThi = bt.NgayThi,      // DateOnly?
                        thu = bt.Thứ,
                        gioBatDau = bt.GioBatDau,  // TimeOnly?
                        gioKetThuc = bt.GioKetThuc,
                        hinhThuc = bt.HinhThuc,
                        phongHoc = ph != null ? ph.TenPhong : null,
                        maPhong = ph != null ? ph.MaPhong : null,
                        diaChi = ph != null ? ph.DiaChi : null
                    }
                ).ToListAsync();

                return Ok(data);
            }
    }
}
