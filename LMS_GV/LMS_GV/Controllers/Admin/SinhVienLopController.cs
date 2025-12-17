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
    [Route("api/admin/sinh-vien-lop")]
    [Authorize(Roles = "Admin")]
    public class SinhVienLopController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SinhVienLopController(AppDbContext db)
        {
            _db = db;
        }

        public class RegisterRequest
        {
            [Required]
            public int SinhVienId { get; set; }

            [Required]
            public int LopHocId { get; set; }
        }

        // 1. POST / – Đăng ký sinh viên vào lớp
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sv = await _db.HoSoSinhViens
                .FirstOrDefaultAsync(x => x.SinhVienId == req.SinhVienId);
            if (sv == null)
                return BadRequest(new { field = "sinhVienId", message = "Sinh viên không tồn tại" });

            var lop = await _db.LopHocs
                .FirstOrDefaultAsync(x => x.LopHocId == req.LopHocId);
            if (lop == null)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            var exists = await _db.SinhVienLops
                .AnyAsync(x => x.SinhVienId == req.SinhVienId && x.LopHocId == req.LopHocId);
            if (exists)
                return Conflict(new { message = "Sinh viên đã đăng ký lớp này" });

            // Check sĩ số
            if (lop.SiSoToiDa.HasValue && lop.SiSo >= lop.SiSoToiDa.Value)
                return BadRequest(new { message = "Lớp đã đầy, không thể đăng ký thêm" });

            var entity = new SinhVienLop
            {
                SinhVienId = req.SinhVienId,
                LopHocId = req.LopHocId,
                NgayVao = DateTime.UtcNow,
                TinhTrang = "dang hoc",
                CreatedAt = DateTime.UtcNow
            };
            _db.SinhVienLops.Add(entity);

            // tăng sĩ số lớp
            lop.SiSo = (lop.SiSo ?? 0) + 1;
            lop.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Register), new { id = entity.SinhVienLopId }, new
            {
                id = entity.SinhVienLopId
            });
        }

        // 2. DELETE /{id} – Hủy đăng ký
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.SinhVienLops
                .FirstOrDefaultAsync(x => x.SinhVienLopId == id);

            if (entity == null)
                return NotFound(new { message = "Không tìm thấy đăng ký lớp" });

            var lop = await _db.LopHocs
                .FirstOrDefaultAsync(l => l.LopHocId == entity.LopHocId);

            _db.SinhVienLops.Remove(entity);

            if (lop != null)
            {
                lop.SiSo = Math.Max(0, (lop.SiSo ?? 0) - 1);
                lop.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
