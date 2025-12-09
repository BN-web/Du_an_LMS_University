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
    [Route("api/admin/bai-hoc")]
    [Authorize(Roles = "Admin")]
    public class BaiHocController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BaiHocController(AppDbContext db)
        {
            _db = db;
        }

        public class CreateBaiHocRequest
        {
            [Required]
            public string TieuDe { get; set; } = string.Empty;

            public string? NoiDung { get; set; }
            public string? LoaiBaiHoc { get; set; } // slide / doc / pdf...
            public int? CreatedBy { get; set; }     // NguoiDung_id
        }

        public class UpdateBaiHocRequest : CreateBaiHocRequest
        {
        }

        // 1. GET / – list
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var list = await (
                from b in _db.BaiHocs.AsNoTracking()
                join nd in _db.NguoiDungs on b.CreatedBy equals nd.NguoiDungId into gnd
                from nd in gnd.DefaultIfEmpty()
                orderby b.CreatedAt descending
                select new
                {
                    id = b.BaiHocId,
                    tieuDe = b.TieuDe,
                    loai = b.LoaiBaiHoc,
                    nguoiTao = nd != null ? nd.HoTen : null,
                    createdAt = b.CreatedAt
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBaiHocRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (req.CreatedBy.HasValue)
            {
                var existsUser = await _db.NguoiDungs.AnyAsync(x => x.NguoiDungId == req.CreatedBy.Value);
                if (!existsUser)
                    return BadRequest(new { field = "createdBy", message = "Người dùng không tồn tại" });
            }

            var entity = new BaiHoc
            {
                TieuDe = req.TieuDe,
                NoiDung = req.NoiDung,
                LoaiBaiHoc = req.LoaiBaiHoc,
                CreatedBy = req.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _db.BaiHocs.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { }, new
            {
                id = entity.BaiHocId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBaiHocRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.BaiHocs
                .FirstOrDefaultAsync(b => b.BaiHocId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy bài học" });

            if (req.CreatedBy.HasValue)
            {
                var existsUser = await _db.NguoiDungs.AnyAsync(x => x.NguoiDungId == req.CreatedBy.Value);
                if (!existsUser)
                    return BadRequest(new { field = "createdBy", message = "Người dùng không tồn tại" });
            }

            entity.TieuDe = req.TieuDe;
            entity.NoiDung = req.NoiDung;
            entity.LoaiBaiHoc = req.LoaiBaiHoc;
            entity.CreatedBy = req.CreatedBy;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.BaiHocs
                .FirstOrDefaultAsync(b => b.BaiHocId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy bài học" });

            var usedInLop = await _db.BaiHocLops.AnyAsync(x => x.BaiHocId == id);
            if (usedInLop)
                return BadRequest(new { message = "Không thể xóa vì bài học đang gắn với lớp" });

            _db.BaiHocs.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
