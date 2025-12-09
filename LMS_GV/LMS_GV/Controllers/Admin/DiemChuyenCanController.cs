using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/diem-chuyen-can")]
    [Authorize(Roles = "Admin")]
    public class DiemChuyenCanController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DiemChuyenCanController(AppDbContext db)
        {
            _db = db;
        }

        public class DiemChuyenCanListQuery
        {
            public int? LopHocId { get; set; }
            public int? SinhVienId { get; set; }
        }

        public class CreateDiemChuyenCanRequest
        {
            [Required]
            public int SinhVienId { get; set; }

            [Required]
            public int LopHocId { get; set; }

            public int? HocKyId { get; set; }

            public decimal? Diem { get; set; } = 10;

            public string? GhiChu { get; set; }
        }

        public class UpdateDiemChuyenCanRequest
        {
            public decimal? Diem { get; set; }
            public string? GhiChu { get; set; }
        }

        // 1. GET /?lopHocId=&sinhVienId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] DiemChuyenCanListQuery queryModel)
        {
            var query = _db.DiemChuyenCans.AsNoTracking().AsQueryable();

            if (queryModel.LopHocId.HasValue)
            {
                var id = queryModel.LopHocId.Value;
                query = query.Where(x => x.LopHocId == id);
            }

            if (queryModel.SinhVienId.HasValue)
            {
                var id = queryModel.SinhVienId.Value;
                query = query.Where(x => x.SinhVienId == id);
            }

            var list = await (
                from d in query
                join sv in _db.HoSoSinhViens on d.SinhVienId equals sv.SinhVienId
                join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                join l in _db.LopHocs on d.LopHocId equals l.LopHocId
                select new
                {
                    id = d.DiemChuyenCanId,
                    sinhVienId = d.SinhVienId,
                    mssv = sv.Mssv,
                    fullName = nd.HoTen,
                    lopHocId = d.LopHocId,
                    maLop = l.MaLop,
                    hocKyId = d.HocKyId,
                    diem = d.Diem,
                    ghiChu = d.GhiChu,
                    createdAt = d.CreatedAt
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDiemChuyenCanRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var svExists = await _db.HoSoSinhViens
                .AnyAsync(s => s.SinhVienId == req.SinhVienId);
            if (!svExists)
                return BadRequest(new { field = "sinhVienId", message = "Sinh viên không tồn tại" });

            var lopExists = await _db.LopHocs.AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            if (req.HocKyId.HasValue)
            {
                var hkExists = await _db.HocKys.AnyAsync(h => h.HocKyId == req.HocKyId.Value);
                if (!hkExists)
                    return BadRequest(new { field = "hocKyId", message = "Học kỳ không tồn tại" });
            }

            var dup = await _db.DiemChuyenCans
                .AnyAsync(d => d.SinhVienId == req.SinhVienId
                               && d.LopHocId == req.LopHocId
                               && d.HocKyId == (req.HocKyId ?? d.HocKyId));
            if (dup)
                return Conflict(new { message = "Đã tồn tại bản ghi chuyên cần cho sinh viên này" });

            var entity = new DiemChuyenCan
            {
                SinhVienId = req.SinhVienId,
                LopHocId = req.LopHocId,
                HocKyId = req.HocKyId ?? 0,
                Diem = req.Diem ?? 10,
                GhiChu = req.GhiChu,
                CreatedAt = DateTime.UtcNow
            };

            _db.DiemChuyenCans.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { lopHocId = req.LopHocId, sinhVienId = req.SinhVienId }, new
            {
                id = entity.DiemChuyenCanId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDiemChuyenCanRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.DiemChuyenCans
                .FirstOrDefaultAsync(d => d.DiemChuyenCanId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy điểm chuyên cần" });

            if (req.Diem.HasValue) entity.Diem = req.Diem.Value;
            if (!string.IsNullOrWhiteSpace(req.GhiChu)) entity.GhiChu = req.GhiChu;

            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.DiemChuyenCans
                .FirstOrDefaultAsync(d => d.DiemChuyenCanId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy điểm chuyên cần" });

            _db.DiemChuyenCans.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
