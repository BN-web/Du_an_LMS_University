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
    [Route("api/admin/khoa-tuyen-sinh")]
    [Authorize(Roles = "Admin")]
    public class KhoaTuyenSinhController : ControllerBase
    {
        private readonly AppDbContext _db;

        public KhoaTuyenSinhController(AppDbContext db)
        {
            _db = db;
        }

        public class CohortListQuery
        {
            public string? Search { get; set; }
            public int? Status { get; set; }   // 1=đang diễn ra, 2=đã tốt nghiệp
            public int? KhoaId { get; set; }  // Lọc theo khoa (dựa trên SV/Nganh/Khoa)
            public int? Year { get; set; }    // Năm bắt đầu hoặc kết thúc
        }

        public class CreateCohortRequest
        {
            [Required]
            [StringLength(10, ErrorMessage = "Mã khóa tối đa 10 ký tự")]
            public string MaKhoa { get; set; } = string.Empty;

            [Required]
            public string TenKhoa { get; set; } = string.Empty;

            [Required]
            public int NamBatDau { get; set; }

            [Required]
            public int NamKetThuc { get; set; }

            public int? KhoaId { get; set; } // Không lưu trực tiếp, dùng cho logic lọc/hiển thị

            public byte? Status { get; set; } // 1/2
        }

        public class UpdateCohortRequest
        {
            [Required]
            [StringLength(10, ErrorMessage = "Mã khóa tối đa 10 ký tự")]
            public string MaKhoa { get; set; } = string.Empty;

            [Required]
            public string TenKhoa { get; set; } = string.Empty;

            [Required]
            public int NamBatDau { get; set; }

            [Required]
            public int NamKetThuc { get; set; }

            public byte? Status { get; set; }
        }

        public class UpdateCohortStatusRequest
        {
            [Required]
            public byte Status { get; set; } // 1=đang diễn ra, 2=đã tốt nghiệp
        }

        // ========== 1. GET / – list ==========

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] CohortListQuery queryModel)
        {
            var query = _db.KhoaTuyenSinhs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var kw = queryModel.Search.Trim();
                query = query.Where(k =>
                    (k.MaKhoaTuyenSinh ?? "").Contains(kw) ||
                    (k.TenKhoaTuyenSinh ?? "").Contains(kw) ||
                    (k.NamBatDau != null && k.NamBatDau.ToString().Contains(kw)) ||
                    (k.NamKetThuc != null && k.NamKetThuc.ToString().Contains(kw)));
            }

            if (queryModel.Status.HasValue)
            {
                var st = (byte)queryModel.Status.Value;
                query = query.Where(k => k.TrangThai == st);
            }

            if (queryModel.Year.HasValue)
            {
                var y = queryModel.Year.Value;
                query = query.Where(k => k.NamBatDau == y || k.NamKetThuc == y);
            }

            if (queryModel.KhoaId.HasValue)
            {
                var khoaId = queryModel.KhoaId.Value;
                query = query.Where(c =>
                    (from sv in _db.HoSoSinhViens
                     join ng in _db.Nganhs on sv.NganhId equals ng.NganhId
                     where sv.KhoaTuyenSinhId == c.KhoaTuyenSinhId && ng.KhoaId == khoaId
                     select sv.SinhVienId).Any()
                    ||
                    (from lh in _db.LopHocs
                     where lh.KhoaTuyenSinhId == c.KhoaTuyenSinhId && lh.KhoaId == khoaId
                     select lh.LopHocId).Any());
            }

            var list = await query
                .OrderByDescending(k => k.NamBatDau)
                .Select(c => new
                {
                    id = c.KhoaTuyenSinhId,
                    code = c.MaKhoaTuyenSinh,
                    name = c.TenKhoaTuyenSinh,
                    yearStart = c.NamBatDau,
                    yearEnd = c.NamKetThuc,
                    status = c.TrangThai,
                    khoaName = (
                        (from sv in _db.HoSoSinhViens
                         join ng in _db.Nganhs on sv.NganhId equals ng.NganhId
                         join k in _db.Khoas on ng.KhoaId equals k.KhoaId
                         where sv.KhoaTuyenSinhId == c.KhoaTuyenSinhId
                         select k.TenKhoa).FirstOrDefault()
                    ) ?? (
                        (from lh in _db.LopHocs
                         join k in _db.Khoas on lh.KhoaId equals k.KhoaId
                         where lh.KhoaTuyenSinhId == c.KhoaTuyenSinhId
                         select k.TenKhoa).FirstOrDefault()
                    ),
                    totalStudents = _db.HoSoSinhViens
                        .Count(sv => sv.KhoaTuyenSinhId == c.KhoaTuyenSinhId),
                    createdAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // ========== 2. GET /{id} – detail ==========

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var cohort = await _db.KhoaTuyenSinhs
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.KhoaTuyenSinhId == id);

            if (cohort == null)
                return NotFound(new { message = "Không tìm thấy khóa tuyển sinh" });

            var totalStudents = await _db.HoSoSinhViens
                .CountAsync(sv => sv.KhoaTuyenSinhId == id);

            var firstKhoa = await (
                from sv in _db.HoSoSinhViens
                join ng in _db.Nganhs on sv.NganhId equals ng.NganhId
                join k in _db.Khoas on ng.KhoaId equals k.KhoaId
                where sv.KhoaTuyenSinhId == cohort.KhoaTuyenSinhId
                select k.TenKhoa
            ).FirstOrDefaultAsync();
            if (firstKhoa == null)
            {
                firstKhoa = await (
                    from lh in _db.LopHocs
                    join k in _db.Khoas on lh.KhoaId equals k.KhoaId
                    where lh.KhoaTuyenSinhId == cohort.KhoaTuyenSinhId
                    select k.TenKhoa
                ).FirstOrDefaultAsync();
            }

            var result = new
            {
                id = cohort.KhoaTuyenSinhId,
                code = cohort.MaKhoaTuyenSinh,
                name = cohort.TenKhoaTuyenSinh,
                yearStart = cohort.NamBatDau,
                yearEnd = cohort.NamKetThuc,
                status = cohort.TrangThai,
                khoaName = firstKhoa,
                totalStudents,
                createdAt = cohort.CreatedAt,
                updatedAt = cohort.UpdatedAt
            };

            return Ok(result);
        }

        // ========== 3. POST / – create ==========

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCohortRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (req.MaKhoa.Length > 10)
                return BadRequest(new { message = "Mã khóa tối đa 10 ký tự" });

            var existsCode = await _db.KhoaTuyenSinhs
                .AnyAsync(k => k.MaKhoaTuyenSinh == req.MaKhoa);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "maKhoa",
                    message = "Mã khóa đã tồn tại"
                });
            }

            if (req.NamBatDau > req.NamKetThuc)
            {
                return BadRequest(new { message = "Năm bắt đầu phải nhỏ hơn hoặc bằng năm kết thúc" });
            }

            var cohort = new KhoaTuyenSinh
            {
                MaKhoaTuyenSinh = req.MaKhoa,
                TenKhoaTuyenSinh = req.TenKhoa,
                NamBatDau = req.NamBatDau,
                NamKetThuc = req.NamKetThuc,
                TrangThai = req.Status ?? 1,
                CreatedAt = DateTime.UtcNow
            };

            _db.KhoaTuyenSinhs.Add(cohort);
            await _db.SaveChangesAsync();

            string? respKhoaName = null;
            if (req.KhoaId.HasValue)
            {
                respKhoaName = await _db.Khoas
                    .Where(k => k.KhoaId == req.KhoaId.Value)
                    .Select(k => k.TenKhoa)
                    .FirstOrDefaultAsync();
            }

            return CreatedAtAction(nameof(GetDetail), new { id = cohort.KhoaTuyenSinhId }, new
            {
                id = cohort.KhoaTuyenSinhId,
                code = cohort.MaKhoaTuyenSinh,
                khoaName = respKhoaName
            });
        }

        // ========== 4. PUT /{id} – update ==========

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCohortRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cohort = await _db.KhoaTuyenSinhs
                .FirstOrDefaultAsync(k => k.KhoaTuyenSinhId == id);

            if (cohort == null)
                return NotFound(new { message = "Không tìm thấy khóa tuyển sinh" });

            if (req.MaKhoa.Length > 10)
                return BadRequest(new { message = "Mã khóa tối đa 10 ký tự" });

            var existsCode = await _db.KhoaTuyenSinhs
                .AnyAsync(k => k.MaKhoaTuyenSinh == req.MaKhoa && k.KhoaTuyenSinhId != id);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "maKhoa",
                    message = "Mã khóa đã tồn tại"
                });
            }

            if (req.NamBatDau > req.NamKetThuc)
            {
                return BadRequest(new { message = "Năm bắt đầu phải nhỏ hơn hoặc bằng năm kết thúc" });
            }

            cohort.MaKhoaTuyenSinh = req.MaKhoa;
            cohort.TenKhoaTuyenSinh = req.TenKhoa;
            cohort.NamBatDau = req.NamBatDau;
            cohort.NamKetThuc = req.NamKetThuc;
            if (req.Status.HasValue)
                cohort.TrangThai = req.Status.Value;
            cohort.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========== 5. PUT /{id}/status – change status ==========

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCohortStatusRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cohort = await _db.KhoaTuyenSinhs
                .FirstOrDefaultAsync(k => k.KhoaTuyenSinhId == id);

            if (cohort == null)
                return NotFound(new { message = "Không tìm thấy khóa tuyển sinh" });

            cohort.TrangThai = req.Status;
            cohort.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========== 6. DELETE /{id} – delete ==========

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cohort = await _db.KhoaTuyenSinhs
                .FirstOrDefaultAsync(k => k.KhoaTuyenSinhId == id);

            if (cohort == null)
                return NotFound(new { message = "Không tìm thấy khóa tuyển sinh" });

            var hasStudents = await _db.HoSoSinhViens
                .AnyAsync(sv => sv.KhoaTuyenSinhId == id);
            if (hasStudents)
            {
                return BadRequest(new
                {
                    message = "Không thể xóa khóa vì đang có sinh viên thuộc khóa này"
                });
            }

            _db.KhoaTuyenSinhs.Remove(cohort);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
