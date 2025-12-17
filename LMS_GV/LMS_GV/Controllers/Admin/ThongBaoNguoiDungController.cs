using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/thong-bao-nguoi-dung")]
    [Authorize(Roles = "Admin")]
    public class ThongBaoNguoiDungController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ThongBaoNguoiDungController(AppDbContext db)
        {
            _db = db;
        }

        public class CreateThongBaoNguoiDungRequest
        {
            [Required]
            public int ThongBaoId { get; set; }

            [Required]
            public int NguoiDungId { get; set; }
        }

        public class UpdateThongBaoNguoiDungRequest
        {
            public bool? DaDoc { get; set; }
        }

        // 1. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateThongBaoNguoiDungRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tbExists = await _db.ThongBaos
                .AnyAsync(t => t.ThongBaoId == req.ThongBaoId);
            if (!tbExists)
                return BadRequest(new { field = "thongBaoId", message = "Thông báo không tồn tại" });

            var ndExists = await _db.NguoiDungs
                .AnyAsync(n => n.NguoiDungId == req.NguoiDungId);
            if (!ndExists)
                return BadRequest(new { field = "nguoiDungId", message = "Người dùng không tồn tại" });

            var dup = await _db.ThongBaoNguoiDungs
                .AnyAsync(x => x.ThongBaoId == req.ThongBaoId && x.NguoiDungId == req.NguoiDungId);
            if (dup)
                return Conflict(new { message = "Người nhận đã được gắn với thông báo này" });

            var entity = new ThongBaoNguoiDung
            {
                ThongBaoId = req.ThongBaoId,
                NguoiDungId = req.NguoiDungId,
                DaDoc = false,
                CreatedAt = System.DateTime.UtcNow
            };

            _db.ThongBaoNguoiDungs.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new { id = entity.ThongBaoNguoiDungId });
        }

        // 2. PUT /{id} – đánh dấu đã đọc
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateThongBaoNguoiDungRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.ThongBaoNguoiDungs
                .FirstOrDefaultAsync(x => x.ThongBaoNguoiDungId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy bản ghi người nhận thông báo" });

            if (req.DaDoc.HasValue)
                entity.DaDoc = req.DaDoc.Value;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
