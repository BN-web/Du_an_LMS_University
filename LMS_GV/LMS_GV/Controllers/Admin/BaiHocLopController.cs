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
    [Route("api/admin/bai-hoc-lop")]
    [Authorize(Roles = "Admin")]
    public class BaiHocLopController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BaiHocLopController(AppDbContext db)
        {
            _db = db;
        }

        public class CreateBaiHocLopRequest
        {
            [Required]
            public int BaiHocId { get; set; }

            [Required]
            public int LopHocId { get; set; }

            public int? ViTri { get; set; }
        }

        // 1. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBaiHocLopRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var baiHocExists = await _db.BaiHocs.AnyAsync(b => b.BaiHocId == req.BaiHocId);
            if (!baiHocExists)
                return BadRequest(new { field = "baiHocId", message = "Bài học không tồn tại" });

            var lopExists = await _db.LopHocs.AnyAsync(l => l.LopHocId == req.LopHocId);
            if (!lopExists)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            var exists = await _db.BaiHocLops
                .AnyAsync(x => x.BaiHocId == req.BaiHocId && x.LopHocId == req.LopHocId);
            if (exists)
                return Conflict(new { message = "Bài học đã được gắn với lớp" });

            var last = (await _db.BaiHocLops
                .Where(x => x.LopHocId == req.LopHocId)
                .Select(x => x.ViTri)
                .DefaultIfEmpty(0)
                .MaxAsync()) ?? 0;

            int viTri = req.ViTri ?? (last + 1);

            var entity = new BaiHocLop
            {
                BaiHocId = req.BaiHocId,
                LopHocId = req.LopHocId,
                ViTri = viTri,
                CreatedAt = DateTime.UtcNow
            };

            _db.BaiHocLops.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Create), new { id = entity.BaiHocLopId }, new
            {
                id = entity.BaiHocLopId
            });
        }

        // 2. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.BaiHocLops
                .FirstOrDefaultAsync(x => x.BaiHocLopId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy liên kết bài học - lớp" });

            _db.BaiHocLops.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
