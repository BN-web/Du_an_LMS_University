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
    [Route("api/admin/giang-vien-lop")]
    [Authorize(Roles = "Admin")]
    public class GiangVienLopController : ControllerBase
    {
        private readonly AppDbContext _db;

        public GiangVienLopController(AppDbContext db)
        {
            _db = db;
        }

        public class AssignRequest
        {
            [Required]
            public int GiangVienId { get; set; }

            [Required]
            public int LopHocId { get; set; }
        }

        // 1. POST / – phân công GV vào lớp
        [HttpPost]
        public async Task<IActionResult> Assign([FromBody] AssignRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gv = await _db.GiangViens
                .Include(x => x.NguoiDung)
                .FirstOrDefaultAsync(x => x.GiangVienId == req.GiangVienId);
            if (gv == null)
                return BadRequest(new { field = "giangVienId", message = "Giảng viên không tồn tại" });

            var lop = await _db.LopHocs
                .FirstOrDefaultAsync(x => x.LopHocId == req.LopHocId);
            if (lop == null)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            var exists = await _db.GiangVienLops
                .AnyAsync(x => x.GiangVienId == req.GiangVienId && x.LopHocId == req.LopHocId);
            if (exists)
                return Conflict(new { message = "Giảng viên đã được phân công vào lớp này" });

            var entity = new GiangVienLop
            {
                GiangVienId = req.GiangVienId,
                LopHocId = req.LopHocId,
                TenGiangVien = gv.NguoiDung?.HoTen,
                NgayPhanCong = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.GiangVienLops.Add(entity);

            // Optionally set LopHoc.GiangVienId
            lop.GiangVienId = req.GiangVienId;
            lop.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Assign), new { id = entity.GiangVienLopId }, new
            {
                id = entity.GiangVienLopId
            });
        }

        // 2. DELETE /{id} – hủy phân công
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.GiangVienLops
                .FirstOrDefaultAsync(x => x.GiangVienLopId == id);

            if (entity == null)
                return NotFound(new { message = "Không tìm thấy phân công" });

            _db.GiangVienLops.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
