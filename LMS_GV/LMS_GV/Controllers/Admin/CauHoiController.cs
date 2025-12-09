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
    [Route("api/admin/cau-hoi")]
    [Authorize(Roles = "Admin")]
    public class CauHoiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CauHoiController(AppDbContext db)
        {
            _db = db;
        }

        public class CauHoiListQuery
        {
            public int? BaiKiemTraId { get; set; }
        }

        public class CreateCauHoiRequest
        {
            [Required]
            public int BaiKiemTraId { get; set; }

            [Required]
            public string NoiDung { get; set; } = string.Empty;

            public string? Loai { get; set; } = "single"; // single/multiple...

            public decimal? Diem { get; set; }
        }

        public class UpdateCauHoiRequest : CreateCauHoiRequest
        {
        }

        // 1. GET /?baiKiemTraId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] CauHoiListQuery queryModel)
        {
            var query = _db.CauHois.AsNoTracking().AsQueryable();

            if (queryModel.BaiKiemTraId.HasValue)
            {
                var id = queryModel.BaiKiemTraId.Value;
                query = query.Where(x => x.BaiKiemTraId == id);
            }

            var list = await query
                .OrderBy(x => x.CauHoiId)
                .Select(x => new
                {
                    id = x.CauHoiId,
                    baiKiemTraId = x.BaiKiemTraId,
                    noiDung = x.NoiDung,
                    loai = x.Loai,
                    diem = x.Diem,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCauHoiRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existsBkt = await _db.BaiKiemTras
                .AnyAsync(b => b.BaiKiemTraId == req.BaiKiemTraId);
            if (!existsBkt)
                return BadRequest(new { field = "baiKiemTraId", message = "Bài kiểm tra không tồn tại" });

            var entity = new CauHoi
            {
                BaiKiemTraId = req.BaiKiemTraId,
                NoiDung = req.NoiDung,
                Loai = req.Loai,
                Diem = req.Diem,
                CreatedAt = DateTime.UtcNow
            };

            _db.CauHois.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { baiKiemTraId = req.BaiKiemTraId }, new
            {
                id = entity.CauHoiId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCauHoiRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.CauHois
                .FirstOrDefaultAsync(c => c.CauHoiId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy câu hỏi" });

            var existsBkt = await _db.BaiKiemTras
                .AnyAsync(b => b.BaiKiemTraId == req.BaiKiemTraId);
            if (!existsBkt)
                return BadRequest(new { field = "baiKiemTraId", message = "Bài kiểm tra không tồn tại" });

            entity.BaiKiemTraId = req.BaiKiemTraId;
            entity.NoiDung = req.NoiDung;
            entity.Loai = req.Loai;
            entity.Diem = req.Diem;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.CauHois
                .FirstOrDefaultAsync(c => c.CauHoiId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy câu hỏi" });

            var hasOptions = await _db.TuyChonCauHois
                .AnyAsync(o => o.CauHoiId == id);
            if (hasOptions)
                return BadRequest(new { message = "Không thể xoá vì câu hỏi còn đáp án" });

            var hasAnswers = await _db.ChiTietCauTraLois
                .AnyAsync(a => a.CauHoiId == id);
            if (hasAnswers)
                return BadRequest(new { message = "Không thể xoá vì câu hỏi đã có câu trả lời" });

            _db.CauHois.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
