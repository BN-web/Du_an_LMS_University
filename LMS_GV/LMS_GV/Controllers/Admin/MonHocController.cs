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
    [Route("api/admin/mon-hoc")]
    [Authorize(Roles = "Admin")]
    public class MonHocController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MonHocController(AppDbContext db)
        {
            _db = db;
        }

        public class MonHocListQuery
        {
            public string? Search { get; set; }
            public int? KhoaId { get; set; }
            public int? NganhId { get; set; }
            public byte? Status { get; set; }   // 1=đang diễn ra, 2=đã kết thúc (theo DB là đang học/đã hoàn thành)
            public int? HocKyId { get; set; }
        }

        public class CreateMonHocRequest
        {
            [Required]
            [StringLength(10, ErrorMessage = "Mã môn tối đa 10 ký tự")]
            public string MaMon { get; set; } = string.Empty;

            [Required]
            public string TenMon { get; set; } = string.Empty;

            [Required]
            public int SoTinChi { get; set; }

            public int? KhoaId { get; set; }    // không lưu trực tiếp, gắn qua lớp
            public int? NganhId { get; set; }   // không lưu trực tiếp
            public int? HocKyId { get; set; }   // không lưu trực tiếp

            public byte? Status { get; set; }   // 1/2
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
        }

        public class UpdateMonHocRequest
        {
            [Required]
            [StringLength(10, ErrorMessage = "Mã môn tối đa 10 ký tự")]
            public string MaMon { get; set; } = string.Empty;

            [Required]
            public string TenMon { get; set; } = string.Empty;

            [Required]
            public int SoTinChi { get; set; }

            public byte? Status { get; set; }
        }

        public class UpdateMonHocStatusRequest
        {
            [Required]
            public byte Status { get; set; } // 1=đang diễn ra, 2=đã kết thúc
        }

        // ========== 1. GET / – list ==========

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] MonHocListQuery queryModel)
        {
            var query = _db.MonHocs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var kw = queryModel.Search.Trim();
                query = query.Where(m =>
                    (m.MaMon ?? "").Contains(kw) ||
                    (m.TenMon ?? "").Contains(kw));
            }

            if (queryModel.Status.HasValue)
            {
                var st = queryModel.Status.Value;
                query = query.Where(m => m.TrangThai == st);
            }

            if (queryModel.KhoaId.HasValue)
            {
                var kId = queryModel.KhoaId.Value;
                // Chỉ lấy các môn có ít nhất 1 lớp thuộc khoa này
                query = query.Where(m =>
                    _db.LopHocs.Any(l =>
                        l.MonHocId == m.MonHocId &&
                        l.KhoaId == kId));
            }

            if (queryModel.NganhId.HasValue)
            {
                var nId = queryModel.NganhId.Value;
                query = query.Where(m =>
                    _db.LopHocs.Any(l =>
                        l.MonHocId == m.MonHocId &&
                        l.NganhId == nId));
            }

            if (queryModel.HocKyId.HasValue)
            {
                var hkId = queryModel.HocKyId.Value;
                query = query.Where(m =>
                    _db.LopHocs.Any(l =>
                        l.MonHocId == m.MonHocId &&
                        l.HocKyId == hkId));
            }

                var list = await query
                    .OrderBy(m => m.TenMon)
                    .Select(m => new
                    {
                        id = m.MonHocId,
                        code = m.MaMon,
                        name = m.TenMon,
                        soTinChi = m.SoTinChi,
                        status = m.TrangThai,
                        createdAt = m.CreatedAt,
                        khoaName = (
                            (from l in _db.LopHocs
                             join k in _db.Khoas on l.KhoaId equals k.KhoaId
                             where l.MonHocId == m.MonHocId
                             select k.TenKhoa).Distinct().FirstOrDefault()
                        ) ?? (
                            (from ct in _db.ChuongTrinhDaoTaos
                             join ctmh in _db.ChuongTrinhMonHocs on ct.ChuongTrinhDaoTaoId equals ctmh.ChuongTrinhDaoTaoId
                             join ng in _db.Nganhs on ct.NganhId equals ng.NganhId
                             join k in _db.Khoas on ng.KhoaId equals k.KhoaId
                             where ctmh.MonHocId == m.MonHocId
                             select k.TenKhoa).Distinct().FirstOrDefault()
                        ),
                        hocKy = (
                            (from l in _db.LopHocs
                             join hk in _db.HocKys on l.HocKyId equals hk.HocKyId
                             where l.MonHocId == m.MonHocId
                             select hk.KiHoc).Distinct().FirstOrDefault()
                        )
                    })
                    .ToListAsync();

            return Ok(list);
        }

        // ========== 2. GET /{id} – detail ==========

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var mon = await _db.MonHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MonHocId == id);

            if (mon == null)
                return NotFound(new { message = "Không tìm thấy môn học" });

            // Thông tin khoa/học kỳ đầu tiên (nếu có)
            var firstClassInfo = await (
                from l in _db.LopHocs
                join k in _db.Khoas on l.KhoaId equals k.KhoaId into gk
                from k in gk.DefaultIfEmpty()
                join hk in _db.HocKys on l.HocKyId equals hk.HocKyId into ghk
                from hk in ghk.DefaultIfEmpty()
                where l.MonHocId == id
                select new
                {
                    khoaName = k != null ? k.TenKhoa : null,
                    hocKy = hk != null ? hk.KiHoc : null,
                    namHoc = hk != null ? hk.NamHoc : null,
                    startDate = l.NgayBatDau,
                    endDate = l.NgayKetThuc
                }
            ).FirstOrDefaultAsync();

            var fallbackKhoaName = await (
                from ct in _db.ChuongTrinhDaoTaos
                join ctmh in _db.ChuongTrinhMonHocs on ct.ChuongTrinhDaoTaoId equals ctmh.ChuongTrinhDaoTaoId
                join ng in _db.Nganhs on ct.NganhId equals ng.NganhId
                join k in _db.Khoas on ng.KhoaId equals k.KhoaId
                where ctmh.MonHocId == id
                select k.TenKhoa
            ).Distinct().FirstOrDefaultAsync();

            // Giảng viên: lấy distinct theo LopHoc.GiangVienId
            var giangVien = await (
                from l in _db.LopHocs
                join gv in _db.GiangViens on l.GiangVienId equals gv.GiangVienId
                join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                where l.MonHocId == id
                select new
                {
                    name = nd.HoTen,
                    email = nd.Email
                }
            ).Distinct().ToListAsync();

            // Lớp thuộc môn này
            var lopList = await (
                from l in _db.LopHocs
                join hk in _db.HocKys on l.HocKyId equals hk.HocKyId
                where l.MonHocId == id
                select new
                {
                    lopHocId = l.LopHocId,
                    maLop = l.MaLop,
                    tenLop = l.TenLop,
                    hocKyId = l.HocKyId,
                    hocKy = hk.KiHoc,
                    namHoc = hk.NamHoc,
                    trangThai = l.TrangThai,
                    soTinChi = l.SoTinChi
                }
            ).ToListAsync();

            // Tài liệu: dựa trên BaiHoc_Lop -> BaiHoc
            var documents = await (
                from l in _db.LopHocs
                join bl in _db.BaiHocLops on l.LopHocId equals bl.LopHocId
                join b in _db.BaiHocs on bl.BaiHocId equals b.BaiHocId
                where l.MonHocId == id
                select new
                {
                    baiHocId = b.BaiHocId,
                    tieuDe = b.TieuDe,
                    loai = b.LoaiBaiHoc,
                    createdAt = b.CreatedAt
                }
            ).Distinct().ToListAsync();

            // Số sinh viên đang học môn này (lớp đang học)
            var studentsCount = await (
                from sl in _db.SinhVienLops
                join l in _db.LopHocs on sl.LopHocId equals l.LopHocId
                where l.MonHocId == id && l.TrangThai == 1
                select sl.SinhVienId
            ).Distinct().CountAsync();

            var result = new
            {
                id = mon.MonHocId,
                code = mon.MaMon,
                name = mon.TenMon,
                soTinChi = mon.SoTinChi,
                status = mon.TrangThai,
                khoaName = firstClassInfo?.khoaName ?? fallbackKhoaName,
                hocKy = firstClassInfo?.hocKy,
                namHoc = firstClassInfo?.namHoc,
                startDate = firstClassInfo?.startDate,
                endDate = firstClassInfo?.endDate,
                lecturers = giangVien,
                lopHoc = lopList,
                documents,
                studentsCount,
                createdAt = mon.CreatedAt,
                updatedAt = mon.UpdatedAt
            };

            return Ok(result);
        }

        // ========== 3. POST / – create ==========

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMonHocRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (req.MaMon.Length > 10)
                return BadRequest(new { message = "Mã môn tối đa 10 ký tự" });

            var existsCode = await _db.MonHocs
                .AnyAsync(m => m.MaMon == req.MaMon);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "maMon",
                    message = "Mã môn đã tồn tại"
                });
            }

            var mon = new MonHoc
            {
                MaMon = req.MaMon,
                TenMon = req.TenMon,
                SoTinChi = req.SoTinChi,
                TrangThai = req.Status ?? 1, // 1=đang diễn ra/đang học
                CreatedAt = DateTime.UtcNow
            };

            _db.MonHocs.Add(mon);
            await _db.SaveChangesAsync();

            if (req.KhoaId.HasValue && req.NganhId.HasValue && req.HocKyId.HasValue)
            {
                var existsKhoa = await _db.Khoas.AnyAsync(k => k.KhoaId == req.KhoaId.Value);
                var existsNganh = await _db.Nganhs.AnyAsync(n => n.NganhId == req.NganhId.Value);
                var existsHocKy = await _db.HocKys.AnyAsync(h => h.HocKyId == req.HocKyId.Value);

                if (existsKhoa && existsNganh && existsHocKy)
                {
                    var maLopGen = "LP" + mon.MonHocId;
                    var maLop = maLopGen.Length <= 10 ? maLopGen : maLopGen.Substring(0, 10);

                    var lop = new LopHoc
                    {
                        MaLop = maLop,
                        TenLop = mon.TenMon,
                        MonHocId = mon.MonHocId,
                        HocKyId = req.HocKyId.Value,
                        KhoaId = req.KhoaId.Value,
                        NganhId = req.NganhId.Value,
                        SoTinChi = req.SoTinChi,
                        NgayBatDau = req.StartDate,
                        NgayKetThuc = req.EndDate,
                        SiSo = 0,
                        TrangThai = 1,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.LopHocs.Add(lop);
                    await _db.SaveChangesAsync();
                }
            }

            return CreatedAtAction(nameof(GetDetail), new { id = mon.MonHocId }, new
            {
                id = mon.MonHocId,
                code = mon.MaMon
            });
        }

        // ========== 4. PUT /{id} – update ==========

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMonHocRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mon = await _db.MonHocs
                .FirstOrDefaultAsync(m => m.MonHocId == id);
            if (mon == null)
                return NotFound(new { message = "Không tìm thấy môn học" });

            if (req.MaMon.Length > 10)
                return BadRequest(new { message = "Mã môn tối đa 10 ký tự" });

            var existsCode = await _db.MonHocs
                .AnyAsync(m => m.MaMon == req.MaMon && m.MonHocId != id);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "maMon",
                    message = "Mã môn đã tồn tại"
                });
            }

            mon.MaMon = req.MaMon;
            mon.TenMon = req.TenMon;
            mon.SoTinChi = req.SoTinChi;
            if (req.Status.HasValue)
                mon.TrangThai = req.Status.Value;
            mon.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========== 5. PUT /{id}/status – change status ==========

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateMonHocStatusRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mon = await _db.MonHocs
                .FirstOrDefaultAsync(m => m.MonHocId == id);
            if (mon == null)
                return NotFound(new { message = "Không tìm thấy môn học" });

            mon.TrangThai = req.Status;
            mon.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========== 6. DELETE /{id} – delete ==========

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var mon = await _db.MonHocs
                .FirstOrDefaultAsync(m => m.MonHocId == id);
            if (mon == null)
                return NotFound(new { message = "Không tìm thấy môn học" });

            var usedInClass = await _db.LopHocs
                .AnyAsync(l => l.MonHocId == id);
            var usedInProgram = await _db.ChuongTrinhMonHocs
                .AnyAsync(ct => ct.MonHocId == id);

            if (usedInClass || usedInProgram)
            {
                return BadRequest(new
                {
                    message = "Không thể xóa môn vì đang được sử dụng trong lớp hoặc CTĐT"
                });
            }

            _db.MonHocs.Remove(mon);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========== 7. GET /{id}/lop-hoc – lớp thuộc môn ==========

        [HttpGet("{id:int}/lop-hoc")]
        public async Task<IActionResult> GetLopHoc(int id)
        {
            var exists = await _db.MonHocs
                .AnyAsync(m => m.MonHocId == id);
            if (!exists)
                return NotFound(new { message = "Không tìm thấy môn học" });

            var data = await (
                from l in _db.LopHocs
                join hk in _db.HocKys on l.HocKyId equals hk.HocKyId
                join k in _db.Khoas on l.KhoaId equals k.KhoaId into gk
                from k in gk.DefaultIfEmpty()
                where l.MonHocId == id
                select new
                {
                    lopHocId = l.LopHocId,
                    maLop = l.MaLop,
                    tenLop = l.TenLop,
                    khoaName = k != null ? k.TenKhoa : null,
                    hocKy = hk.KiHoc,
                    namHoc = hk.NamHoc,
                    soTinChi = l.SoTinChi,
                    trangThai = l.TrangThai
                }
            ).ToListAsync();

            return Ok(data);
        }
    }
}
