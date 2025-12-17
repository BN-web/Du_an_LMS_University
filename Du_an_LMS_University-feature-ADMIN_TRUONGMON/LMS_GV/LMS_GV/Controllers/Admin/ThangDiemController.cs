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
    [Route("api/admin/thang-diem")]
    [Authorize(Roles = "Admin")]
    public class ThangDiemController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ThangDiemController(AppDbContext db)
        {
            _db = db;
        }

        public class CreateThangDiemRequest
        {
            [Required]
            public string DiemChu { get; set; } = string.Empty;

            [Required]
            public decimal DiemMin { get; set; }

            [Required]
            public decimal DiemMax { get; set; }
        }

        public class UpdateThangDiemRequest : CreateThangDiemRequest
        {
        }

        // 1. GET /
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var list = await _db.ThangDiems
                .AsNoTracking()
                .OrderByDescending(x => x.DiemMin)
                .Select(x => new
                {
                    id = x.ThangDiemId,
                    diemChu = x.DiemChu,
                    diemMin = x.DiemMin,
                    diemMax = x.DiemMax,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateThangDiemRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = new ThangDiem
            {
                DiemChu = req.DiemChu,
                DiemMin = req.DiemMin,
                DiemMax = req.DiemMax,
                CreatedAt = DateTime.UtcNow
            };

            _db.ThangDiems.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { }, new
            {
                id = entity.ThangDiemId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateThangDiemRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.ThangDiems
                .FirstOrDefaultAsync(x => x.ThangDiemId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy thang điểm" });

            entity.DiemChu = req.DiemChu;
            entity.DiemMin = req.DiemMin;
            entity.DiemMax = req.DiemMax;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.ThangDiems
                .FirstOrDefaultAsync(x => x.ThangDiemId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy thang điểm" });

            _db.ThangDiems.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
