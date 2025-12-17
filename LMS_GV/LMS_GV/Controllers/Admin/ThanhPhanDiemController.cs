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
    [Route("api/admin/thanh-phan-diem")]
    [Authorize(Roles = "Admin")]
    public class ThanhPhanDiemController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ThanhPhanDiemController(AppDbContext db)
        {
            _db = db;
        }

        public class ThanhPhanDiemListQuery
        {
            public int? LopHocId { get; set; }
        }

        public class CreateThanhPhanRequest
        {
            [Required]
            public int LopHocId { get; set; }

            [Required]
            public string Ten { get; set; } = string.Empty;

            [Required]
            public decimal HeSo { get; set; }
        }

        public class UpdateThanhPhanRequest : CreateThanhPhanRequest
        {
        }

        // 1. GET /?lopHocId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] ThanhPhanDiemListQuery queryModel)
        {
            var query = _db.ThanhPhanDiems.AsNoTracking().AsQueryable();

            if (queryModel.LopHocId.HasValue)
            {
                var id = queryModel.LopHocId.Value;
                query = query.Where(x => x.LopHocId == id);
            }

            var list = await query
                .OrderBy(x => x.ThanhPhanDiemId)
                .Select(x => new
                {
                    id = x.ThanhPhanDiemId,
                    lopHocId = x.LopHocId,
                    ten = x.Ten,
                    heSo = x.HeSo,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateThanhPhanRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lopExists = await _db.LopHocs
                .AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            var entity = new ThanhPhanDiem
            {
                LopHocId = req.LopHocId,
                Ten = req.Ten,
                HeSo = req.HeSo,
                CreatedAt = DateTime.UtcNow
            };

            _db.ThanhPhanDiems.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { lopHocId = req.LopHocId }, new
            {
                id = entity.ThanhPhanDiemId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateThanhPhanRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.ThanhPhanDiems
                .FirstOrDefaultAsync(x => x.ThanhPhanDiemId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy cấu hình điểm" });

            var lopExists = await _db.LopHocs
                .AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            entity.LopHocId = req.LopHocId;
            entity.Ten = req.Ten;
            entity.HeSo = req.HeSo;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.ThanhPhanDiems
                .FirstOrDefaultAsync(x => x.ThanhPhanDiemId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy cấu hình điểm" });

            var used = await _db.DiemThanhPhans
                .AnyAsync(d => d.ThanhPhanDiemId == id);
            if (used)
                return BadRequest(new { message = "Không thể xoá vì đang có điểm thành phần gắn với cấu hình này" });

            _db.ThanhPhanDiems.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
