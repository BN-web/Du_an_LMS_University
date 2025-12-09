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
    [Route("api/admin/buoi-hoc")]
    [Authorize(Roles = "Admin")]
    public class BuoiHocController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BuoiHocController(AppDbContext db)
        {
            _db = db;
        }

        public class BuoiHocListQuery
        {
            public int? LopHocId { get; set; }
        }

        public class CreateBuoiHocRequest
        {
            [Required]
            public int LopHocId { get; set; }

            public int? PhongHocId { get; set; }

            [Required]
            public DateTime StartTime { get; set; }

            [Required]
            public DateTime EndTime { get; set; }

            public string? Thu { get; set; }        // nếu không truyền, tự tính từ StartTime
            public string? LoaiBuoiHoc { get; set; } = "giang bai";
            public int? SoBuoi { get; set; }
            public byte? TrangThai { get; set; }
            public string? GhiChu { get; set; }
        }

        public class UpdateBuoiHocRequest : CreateBuoiHocRequest
        {
        }

        // 1. GET /?lopHocId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] BuoiHocListQuery queryModel)
        {
            var query = _db.BuoiHocs
                .AsNoTracking()
                .AsQueryable();

            if (queryModel.LopHocId.HasValue)
            {
                var id = queryModel.LopHocId.Value;
                query = query.Where(x => x.LopHocId == id);
            }

            var list = await (
                from bh in query
                join ph in _db.PhongHocs on bh.PhongHocId equals ph.PhongHocId into gph
                from ph in gph.DefaultIfEmpty()
                orderby bh.ThoiGianBatDau
                select new
                {
                    id = bh.BuoiHocId,
                    lopHocId = bh.LopHocId,
                    phongHocId = bh.PhongHocId,
                    phong = ph != null ? ph.TenPhong : null,
                    maPhong = ph != null ? ph.MaPhong : null,
                    thu = bh.Thứ,
                    startTime = bh.ThoiGianBatDau,
                    endTime = bh.ThoiGianKetThuc,
                    loai = bh.LoaiBuoiHoc,
                    trangThai = bh.TrangThai,
                    soBuoi = bh.SoBuoi,
                    ghiChu = bh.GhiChu
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. POST / – tạo buổi học
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBuoiHocRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lopExists = await _db.LopHocs
                .AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            if (req.PhongHocId.HasValue)
            {
                var roomExists = await _db.PhongHocs
                    .AnyAsync(p => p.PhongHocId == req.PhongHocId.Value);
                if (!roomExists)
                    return BadRequest(new { field = "phongHocId", message = "Phòng học không tồn tại" });
            }

            var thu = !string.IsNullOrWhiteSpace(req.Thu)
                ? req.Thu
                : req.StartTime.DayOfWeek.ToString();

            var entity = new BuoiHoc
            {
                LopHocId = req.LopHocId,
                PhongHocId = req.PhongHocId,
                Thứ = thu,
                ThoiGianBatDau = req.StartTime,
                ThoiGianKetThuc = req.EndTime,
                LoaiBuoiHoc = req.LoaiBuoiHoc ?? "giang bai",
                TrangThai = req.TrangThai ?? 1,
                SoBuoi = req.SoBuoi ?? 1,
                GhiChu = req.GhiChu,
                CreatedAt = DateTime.UtcNow
            };

            _db.BuoiHocs.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { lopHocId = req.LopHocId }, new
            {
                id = entity.BuoiHocId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBuoiHocRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.BuoiHocs
                .FirstOrDefaultAsync(b => b.BuoiHocId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy buổi học" });

            var lopExists = await _db.LopHocs.AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            if (req.PhongHocId.HasValue)
            {
                var roomExists = await _db.PhongHocs
                    .AnyAsync(p => p.PhongHocId == req.PhongHocId.Value);
                if (!roomExists)
                    return BadRequest(new { field = "phongHocId", message = "Phòng học không tồn tại" });
            }

            entity.LopHocId = req.LopHocId;
            entity.PhongHocId = req.PhongHocId;
            entity.Thứ = string.IsNullOrWhiteSpace(req.Thu)
                ? req.StartTime.DayOfWeek.ToString()
                : req.Thu;
            entity.ThoiGianBatDau = req.StartTime;
            entity.ThoiGianKetThuc = req.EndTime;
            entity.LoaiBuoiHoc = req.LoaiBuoiHoc ?? "giang bai";
            entity.TrangThai = req.TrangThai ?? entity.TrangThai;
            entity.SoBuoi = req.SoBuoi ?? entity.SoBuoi;
            entity.GhiChu = req.GhiChu;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.BuoiHocs
                .FirstOrDefaultAsync(b => b.BuoiHocId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy buổi học" });

            _db.BuoiHocs.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
