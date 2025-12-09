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
    [Route("api/admin/buoi-thi")]
    [Authorize(Roles = "Admin")]
    public class BuoiThiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BuoiThiController(AppDbContext db)
        {
            _db = db;
        }

        public class BuoiThiListQuery
        {
            public int? LopHocId { get; set; }
        }

        public class CreateBuoiThiRequest
        {
            [Required]
            public int LopHocId { get; set; }

            public int? PhongHocId { get; set; }

            // FE truyền kiểu ISO: "2025-01-10T08:00:00"
            [Required]
            public DateTime StartTime { get; set; }

            [Required]
            public DateTime EndTime { get; set; }

            public string? HinhThuc { get; set; } // online/offline
            public int? GiamThiId { get; set; }   // GiangVien
        }

        public class UpdateBuoiThiRequest : CreateBuoiThiRequest
        {
        }

        // 1. GET /?lopHocId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] BuoiThiListQuery queryModel)
        {
            var query = _db.BuoiThis.AsNoTracking().AsQueryable();

            if (queryModel.LopHocId.HasValue)
            {
                var id = queryModel.LopHocId.Value;
                query = query.Where(x => x.LopHocId == id);
            }

            var list = await (
                from bt in query
                join ph in _db.PhongHocs on bt.PhongHocId equals ph.PhongHocId into gph
                from ph in gph.DefaultIfEmpty()
                orderby bt.NgayThi
                select new
                {
                    id = bt.BuoiThiId,
                    lopHocId = bt.LopHocId,
                    phongHocId = bt.PhongHocId,
                    phong = ph != null ? ph.TenPhong : null,
                    maPhong = ph != null ? ph.MaPhong : null,
                    ngayThi = bt.NgayThi,        // DateOnly? -> FE format theo ý
                    thu = bt.Thứ,
                    gioBatDau = bt.GioBatDau,    // TimeOnly?
                    gioKetThuc = bt.GioKetThuc,  // TimeOnly?
                    hinhThuc = bt.HinhThuc
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBuoiThiRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lopExists = await _db.LopHocs.AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            if (req.PhongHocId.HasValue)
            {
                var roomExists = await _db.PhongHocs.AnyAsync(p => p.PhongHocId == req.PhongHocId.Value);
                if (!roomExists)
                    return BadRequest(new { field = "phongHocId", message = "Phòng học không tồn tại" });
            }

            if (req.GiamThiId.HasValue)
            {
                var gvExists = await _db.GiangViens.AnyAsync(gv => gv.GiangVienId == req.GiamThiId.Value);
                if (!gvExists)
                    return BadRequest(new { field = "giamThiId", message = "Giảng viên không tồn tại" });
            }

            var entity = new BuoiThi
            {
                LopHocId = req.LopHocId,
                PhongHocId = req.PhongHocId,

                // ✅ fix ở đây
                NgayThi = req.StartTime.Date,
                Thứ = req.StartTime.DayOfWeek.ToString(),
                GioBatDau = TimeOnly.FromDateTime(req.StartTime),
                GioKetThuc = TimeOnly.FromDateTime(req.EndTime),

                HinhThuc = req.HinhThuc,
                GiamThiId = req.GiamThiId,
                CreatedAt = DateTime.UtcNow
            };

            _db.BuoiThis.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { lopHocId = req.LopHocId }, new
            {
                id = entity.BuoiThiId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBuoiThiRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.BuoiThis
                .FirstOrDefaultAsync(x => x.BuoiThiId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy buổi thi" });

            var lopExists = await _db.LopHocs.AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            if (req.PhongHocId.HasValue)
            {
                var roomExists = await _db.PhongHocs.AnyAsync(p => p.PhongHocId == req.PhongHocId.Value);
                if (!roomExists)
                    return BadRequest(new { field = "phongHocId", message = "Phòng học không tồn tại" });
            }

            if (req.GiamThiId.HasValue)
            {
                var gvExists = await _db.GiangViens.AnyAsync(gv => gv.GiangVienId == req.GiamThiId.Value);
                if (!gvExists)
                    return BadRequest(new { field = "giamThiId", message = "Giảng viên không tồn tại" });
            }

            entity.LopHocId = req.LopHocId;
            entity.PhongHocId = req.PhongHocId;

            // ✅ fix ở đây
            entity.NgayThi = req.StartTime.Date;
            entity.Thứ = req.StartTime.DayOfWeek.ToString();
            entity.GioBatDau = TimeOnly.FromDateTime(req.StartTime);
            entity.GioKetThuc = TimeOnly.FromDateTime(req.EndTime);

            entity.HinhThuc = req.HinhThuc;
            entity.GiamThiId = req.GiamThiId;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.BuoiThis
                .FirstOrDefaultAsync(x => x.BuoiThiId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy buổi thi" });

            _db.BuoiThis.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}