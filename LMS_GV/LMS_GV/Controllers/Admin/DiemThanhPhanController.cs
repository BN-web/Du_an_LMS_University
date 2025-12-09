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
    [Route("api/admin/diem-thanh-phan")]
    [Authorize(Roles = "Admin")]
    public class DiemThanhPhanController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DiemThanhPhanController(AppDbContext db)
        {
            _db = db;
        }

        public class DiemThanhPhanListQuery
        {
            public int? LopHocId { get; set; }
            public int? SinhVienId { get; set; }
        }

        public class CreateDiemThanhPhanRequest
        {
            [Required]
            public int SinhVienId { get; set; }

            [Required]
            public int LopHocId { get; set; }

            [Required]
            public int ThanhPhanDiemId { get; set; }

            [Required]
            public decimal Diem { get; set; }

            public string? GhiChu { get; set; }
        }

        public class UpdateDiemThanhPhanRequest
        {
            public decimal? Diem { get; set; }
            public string? GhiChu { get; set; }
        }

        // 1. GET /?lopHocId=&sinhVienId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] DiemThanhPhanListQuery queryModel)
        {
            var query = _db.DiemThanhPhans.AsNoTracking().AsQueryable();

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
                join t in _db.ThanhPhanDiems on d.ThanhPhanDiemId equals t.ThanhPhanDiemId
                select new
                {
                    id = d.DiemThanhPhanId,
                    sinhVienId = d.SinhVienId,
                    mssv = sv.Mssv,
                    fullName = nd.HoTen,
                    lopHocId = d.LopHocId,
                    maLop = l.MaLop,
                    thanhPhanDiemId = d.ThanhPhanDiemId,
                    thanhPhanTen = t.Ten,
                    diem = d.Diem,
                    ghiChu = d.GhiChu,
                    createdAt = d.CreatedAt
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDiemThanhPhanRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var svExists = await _db.HoSoSinhViens
                .AnyAsync(s => s.SinhVienId == req.SinhVienId);
            if (!svExists)
                return BadRequest(new { field = "sinhVienId", message = "Sinh viên không tồn tại" });

            var lopExists = await _db.LopHocs
                .AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            var tp = await _db.ThanhPhanDiems
                .FirstOrDefaultAsync(t => t.ThanhPhanDiemId == req.ThanhPhanDiemId);
            if (tp == null)
                return BadRequest(new { field = "thanhPhanDiemId", message = "Thành phần điểm không tồn tại" });

            if (tp.LopHocId != req.LopHocId)
                return BadRequest(new { message = "Thành phần điểm không thuộc lớp này" });

            var dup = await _db.DiemThanhPhans
                .AnyAsync(d => d.SinhVienId == req.SinhVienId
                               && d.LopHocId == req.LopHocId
                               && d.ThanhPhanDiemId == req.ThanhPhanDiemId);
            if (dup)
                return Conflict(new { message = "Đã tồn tại điểm cho sinh viên và thành phần này" });

            var entity = new DiemThanhPhan
            {
                SinhVienId = req.SinhVienId,
                LopHocId = req.LopHocId,
                ThanhPhanDiemId = req.ThanhPhanDiemId,
                Diem = req.Diem,
                GhiChu = req.GhiChu,
                CreatedAt = DateTime.UtcNow
            };

            _db.DiemThanhPhans.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { lopHocId = req.LopHocId, sinhVienId = req.SinhVienId }, new
            {
                id = entity.DiemThanhPhanId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDiemThanhPhanRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.DiemThanhPhans
                .FirstOrDefaultAsync(d => d.DiemThanhPhanId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy điểm thành phần" });

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
            var entity = await _db.DiemThanhPhans
                .FirstOrDefaultAsync(d => d.DiemThanhPhanId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy điểm thành phần" });

            _db.DiemThanhPhans.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
