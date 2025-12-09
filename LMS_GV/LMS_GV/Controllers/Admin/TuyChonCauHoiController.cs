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
    [Route("api/admin/tuy-chon-cau-hoi")]
    [Authorize(Roles = "Admin")]
    public class TuyChonCauHoiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TuyChonCauHoiController(AppDbContext db)
        {
            _db = db;
        }

        public class OptionListQuery
        {
            public int? CauHoiId { get; set; }
        }

        public class CreateOptionRequest
        {
            [Required]
            public int CauHoiId { get; set; }

            [Required]
            public string NoiDung { get; set; } = string.Empty;

            [Required]
            public bool LaDapAn { get; set; }

            public int? ThuTu { get; set; }
        }

        public class UpdateOptionRequest
        {
            [Required]
            public string NoiDung { get; set; } = string.Empty;

            [Required]
            public bool LaDapAn { get; set; }

            public int? ThuTu { get; set; }
        }

        private static string ThuTuToMaLuaChon(int index)
        {
            // 1 -> A, 2 -> B, ...
            index = Math.Max(1, index);
            int charCode = 'A' + (index - 1);
            if (charCode > 'Z') charCode = 'Z';
            return ((char)charCode).ToString();
        }

        // 1. GET /?cauHoiId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] OptionListQuery queryModel)
        {
            var query = _db.TuyChonCauHois.AsNoTracking().AsQueryable();

            if (queryModel.CauHoiId.HasValue)
            {
                var id = queryModel.CauHoiId.Value;
                query = query.Where(x => x.CauHoiId == id);
            }

            var list = await query
                .OrderBy(x => x.ThuTu)
                .Select(x => new
                {
                    id = x.TuyChonCauHoiId,
                    cauHoiId = x.CauHoiId,
                    maLuaChon = x.MaLuaChon,
                    noiDung = x.NoiDung,
                    laDapAn = x.LaDapAn,
                    thuTu = x.ThuTu
                })
                .ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOptionRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var question = await _db.CauHois
                .FirstOrDefaultAsync(c => c.CauHoiId == req.CauHoiId);
            if (question == null)
                return BadRequest(new { field = "cauHoiId", message = "Câu hỏi không tồn tại" });

            var last = (await _db.TuyChonCauHois
                .Where(x => x.CauHoiId == req.CauHoiId)
                .Select(x => x.ThuTu)
                .DefaultIfEmpty(0)
                .MaxAsync()) ?? 0;

            int thuTu = req.ThuTu ?? (last + 1);

            var ma = ThuTuToMaLuaChon(thuTu);

            var entity = new TuyChonCauHoi
            {
                CauHoiId = req.CauHoiId,
                MaLuaChon = ma,
                NoiDung = req.NoiDung,
                LaDapAn = req.LaDapAn,
                ThuTu = thuTu,
                CreatedAt = DateTime.UtcNow
            };

            _db.TuyChonCauHois.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { cauHoiId = req.CauHoiId }, new
            {
                id = entity.TuyChonCauHoiId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOptionRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.TuyChonCauHois
                .FirstOrDefaultAsync(x => x.TuyChonCauHoiId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy lựa chọn" });

            int thuTu = req.ThuTu ?? entity.ThuTu ?? 1;
            entity.NoiDung = req.NoiDung;
            entity.LaDapAn = req.LaDapAn;
            entity.ThuTu = thuTu;
            entity.MaLuaChon = ThuTuToMaLuaChon(thuTu);
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.TuyChonCauHois
                .FirstOrDefaultAsync(x => x.TuyChonCauHoiId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy lựa chọn" });

            _db.TuyChonCauHois.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
