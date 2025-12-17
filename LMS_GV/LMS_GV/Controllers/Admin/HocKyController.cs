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
    [Route("api/admin/hoc-ky")]
    [Authorize(Roles = "Admin")]
    public class HocKyController : ControllerBase
    {
        private readonly AppDbContext _db;

        public HocKyController(AppDbContext db)
        {
            _db = db;
        }

        public class CreateHocKyRequest
        {
            [Required]
            public string NamHoc { get; set; } = string.Empty;

            [Required]
            public string KiHoc { get; set; } = string.Empty;
        }

        public class UpdateHocKyRequest
        {
            [Required]
            public string NamHoc { get; set; } = string.Empty;

            [Required]
            public string KiHoc { get; set; } = string.Empty;
        }

        // ========== 1. GET / – list ==========

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var list = await _db.HocKys
                .AsNoTracking()
                .OrderByDescending(h => h.NamHoc)
                .ThenBy(h => h.KiHoc)
                .Select(h => new
                {
                    id = h.HocKyId,
                    namHoc = h.NamHoc,
                    kiHoc = h.KiHoc,
                    createdAt = h.CreatedAt,
                    updatedAt = h.UpdatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // ========== 2. POST / – create ==========

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHocKyRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _db.HocKys
                .AnyAsync(h => h.NamHoc == req.NamHoc && h.KiHoc == req.KiHoc);
            if (exists)
            {
                return Conflict(new
                {
                    message = "Học kỳ đã tồn tại"
                });
            }

            var hk = new HocKy
            {
                NamHoc = req.NamHoc,
                KiHoc = req.KiHoc,
                CreatedAt = DateTime.UtcNow
            };
            _db.HocKys.Add(hk);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { }, new
            {
                id = hk.HocKyId
            });
        }

        // ========== 3. PUT /{id} – update ==========

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHocKyRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hk = await _db.HocKys
                .FirstOrDefaultAsync(h => h.HocKyId == id);
            if (hk == null)
                return NotFound(new { message = "Không tìm thấy học kỳ" });

            var exists = await _db.HocKys
                .AnyAsync(h => h.NamHoc == req.NamHoc &&
                               h.KiHoc == req.KiHoc &&
                               h.HocKyId != id);
            if (exists)
            {
                return Conflict(new
                {
                    message = "Học kỳ đã tồn tại"
                });
            }

            hk.NamHoc = req.NamHoc;
            hk.KiHoc = req.KiHoc;
            hk.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========== 4. DELETE /{id} – delete ==========

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var hk = await _db.HocKys
                .FirstOrDefaultAsync(h => h.HocKyId == id);
            if (hk == null)
                return NotFound(new { message = "Không tìm thấy học kỳ" });

            var usedInLop = await _db.LopHocs.AnyAsync(l => l.HocKyId == id);
            var usedInDiem = await _db.Diems.AnyAsync(d => d.HocKyId == id);
            var usedInDK = await _db.DangKyTinChis.AnyAsync(dk => dk.HocKyId == id);
            var usedInDiemCC = await _db.DiemChuyenCans.AnyAsync(dc => dc.HocKyId == id);

            if (usedInLop || usedInDiem || usedInDK || usedInDiemCC)
            {
                return BadRequest(new
                {
                    message = "Không thể xóa học kỳ vì đang được sử dụng"
                });
            }

            _db.HocKys.Remove(hk);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
