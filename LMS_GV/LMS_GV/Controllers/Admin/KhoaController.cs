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
    [Route("api/admin/khoa")]
    [Authorize(Roles = "Admin")]
    public class KhoaController : ControllerBase
    {
        private readonly AppDbContext _db;

        public KhoaController(AppDbContext db)
        {
            _db = db;
        }

        public class KhoaListQuery
        {
            public string? Search { get; set; }
        }

        public class CreateKhoaRequest
        {
            [Required]
            [StringLength(10, ErrorMessage = "Mã khoa tối đa 10 ký tự")]
            public string MaKhoa { get; set; } = string.Empty;

            [Required]
            public string TenKhoa { get; set; } = string.Empty;

            // Info thêm, có thể lưu tóm tắt vào MoTa nếu muốn
            public string? TruongKhoa { get; set; }
            public int? SoNganhDuKien { get; set; }
        }

        public class UpdateKhoaRequest
        {
            [Required]
            [StringLength(10, ErrorMessage = "Mã khoa tối đa 10 ký tự")]
            public string MaKhoa { get; set; } = string.Empty;

            [Required]
            public string TenKhoa { get; set; } = string.Empty;

            public string? TruongKhoa { get; set; }
            public int? SoNganhDuKien { get; set; }
        }

        // ========== 1. GET / – list ==========

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] KhoaListQuery queryModel)
        {
            var query = _db.Khoas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var kw = queryModel.Search.Trim();
                query = query.Where(k =>
                    (k.MaKhoa ?? "").Contains(kw) ||
                    (k.TenKhoa ?? "").Contains(kw));
            }

            var list = await query
                .OrderBy(k => k.TenKhoa)
                .Select(k => new
                {
                    id = k.KhoaId,
                    code = k.MaKhoa,
                    name = k.TenKhoa,
                    truongKhoa = (
                        from gv in _db.GiangViens
                        join nd in _db.NguoiDungs
                            on gv.NguoiDungId equals nd.NguoiDungId
                        where gv.KhoaId == k.KhoaId &&
                              gv.ChucVu != null &&
                              gv.ChucVu.Contains("Trưởng khoa")
                        select nd.HoTen
                    ).FirstOrDefault(),
                    totalMajors = _db.Nganhs
                        .Count(n => n.KhoaId == k.KhoaId),
                    createdAt = k.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // ========== 2. GET /{id} – detail ==========

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var khoa = await _db.Khoas
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.KhoaId == id);

            if (khoa == null)
                return NotFound(new { message = "Không tìm thấy khoa" });

            var truongKhoaName = await (
                from gv in _db.GiangViens
                join nd in _db.NguoiDungs
                    on gv.NguoiDungId equals nd.NguoiDungId
                where gv.KhoaId == khoa.KhoaId &&
                      gv.ChucVu != null &&
                      gv.ChucVu.Contains("Trưởng khoa")
                select nd.HoTen
            ).FirstOrDefaultAsync();

            var totalMajors = await _db.Nganhs
                .CountAsync(n => n.KhoaId == khoa.KhoaId);

            var totalLecturers = await _db.GiangViens
                .CountAsync(gv => gv.KhoaId == khoa.KhoaId);

            var totalStudents = await (
                from sv in _db.HoSoSinhViens
                join ng in _db.Nganhs on sv.NganhId equals ng.NganhId
                where ng.KhoaId == khoa.KhoaId
                select sv.SinhVienId
            ).Distinct().CountAsync();

            var result = new
            {
                id = khoa.KhoaId,
                code = khoa.MaKhoa,
                name = khoa.TenKhoa,
                truongKhoa = truongKhoaName,
                totalMajors,
                totalLecturers,
                totalStudents,
                createdAt = khoa.CreatedAt,
                updatedAt = khoa.UpdatedAt,
                moTa = khoa.MoTa
            };

            return Ok(result);
        }

        // ========== 3. POST / – create ==========

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKhoaRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (req.MaKhoa.Length > 10)
                return BadRequest(new { message = "Mã khoa tối đa 10 ký tự" });

            var existsCode = await _db.Khoas
                .AnyAsync(k => k.MaKhoa == req.MaKhoa);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "maKhoa",
                    message = "Mã khoa đã tồn tại"
                });
            }

            string? moTa = null;
            if (!string.IsNullOrWhiteSpace(req.TruongKhoa) || req.SoNganhDuKien.HasValue)
            {
                moTa = $"Trưởng khoa: {req.TruongKhoa ?? "Chưa xác định"}, " +
                       $"Số ngành dự kiến: {req.SoNganhDuKien?.ToString() ?? "Chưa xác định"}";
            }

            var khoa = new Khoa
            {
                MaKhoa = req.MaKhoa,
                TenKhoa = req.TenKhoa,
                MoTa = moTa,
                CreatedAt = DateTime.UtcNow
            };

            _db.Khoas.Add(khoa);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetail), new { id = khoa.KhoaId }, new
            {
                id = khoa.KhoaId,
                code = khoa.MaKhoa
            });
        }

        // ========== 4. PUT /{id} – update ==========

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateKhoaRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var khoa = await _db.Khoas
                .FirstOrDefaultAsync(k => k.KhoaId == id);

            if (khoa == null)
                return NotFound(new { message = "Không tìm thấy khoa" });

            if (req.MaKhoa.Length > 10)
                return BadRequest(new { message = "Mã khoa tối đa 10 ký tự" });

            var existsCode = await _db.Khoas
                .AnyAsync(k => k.MaKhoa == req.MaKhoa && k.KhoaId != id);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "maKhoa",
                    message = "Mã khoa đã tồn tại"
                });
            }

            string? moTa = null;
            if (!string.IsNullOrWhiteSpace(req.TruongKhoa) || req.SoNganhDuKien.HasValue)
            {
                moTa = $"Trưởng khoa: {req.TruongKhoa ?? "Chưa xác định"}, " +
                       $"Số ngành dự kiến: {req.SoNganhDuKien?.ToString() ?? "Chưa xác định"}";
            }

            khoa.MaKhoa = req.MaKhoa;
            khoa.TenKhoa = req.TenKhoa;
            khoa.MoTa = moTa;
            khoa.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========== 5. DELETE /{id} – delete ==========

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var khoa = await _db.Khoas
                .FirstOrDefaultAsync(k => k.KhoaId == id);

            if (khoa == null)
                return NotFound(new { message = "Không tìm thấy khoa" });

            var hasMajors = await _db.Nganhs
                .AnyAsync(n => n.KhoaId == id);
            if (hasMajors)
            {
                return BadRequest(new
                {
                    message = "Không thể xóa khoa vì đang có ngành thuộc khoa này"
                });
            }

            _db.Khoas.Remove(khoa);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
