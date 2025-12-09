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
    [Route("api/admin/phong-hoc")]
    [Authorize(Roles = "Admin")]
    public class PhongHocController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PhongHocController(AppDbContext db)
        {
            _db = db;
        }

        public class CreatePhongHocRequest
        {
            public string? MaPhong { get; set; }

            [Required]
            public string TenPhong { get; set; } = string.Empty;

            public int? SucChua { get; set; }

            public string? DiaChi { get; set; }
        }

        public class UpdatePhongHocRequest : CreatePhongHocRequest
        {
        }

        // 1. GET /
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var list = await _db.PhongHocs
                .AsNoTracking()
                .OrderBy(x => x.MaPhong)
                .Select(x => new
                {
                    id = x.PhongHocId,
                    maPhong = x.MaPhong,
                    tenPhong = x.TenPhong,
                    sucChua = x.SucChua,
                    diaChi = x.DiaChi,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePhongHocRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = new PhongHoc
            {
                MaPhong = req.MaPhong,
                TenPhong = req.TenPhong,
                SucChua = req.SucChua,
                DiaChi = req.DiaChi,
                CreatedAt = DateTime.UtcNow
            };

            _db.PhongHocs.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { }, new
            {
                id = entity.PhongHocId
            });
        }

        // 3. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePhongHocRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.PhongHocs
                .FirstOrDefaultAsync(x => x.PhongHocId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy phòng học" });

            entity.MaPhong = req.MaPhong;
            entity.TenPhong = req.TenPhong;
            entity.SucChua = req.SucChua;
            entity.DiaChi = req.DiaChi;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.PhongHocs
                .FirstOrDefaultAsync(x => x.PhongHocId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy phòng học" });

            var usedInBuoiHoc = await _db.BuoiHocs
                .AnyAsync(b => b.PhongHocId == id);
            var usedInBuoiThi = await _db.BuoiThis
                .AnyAsync(b => b.PhongHocId == id);

            if (usedInBuoiHoc || usedInBuoiThi)
                return BadRequest(new { message = "Phòng đang được sử dụng trong lịch học/thi, không thể xoá" });

            _db.PhongHocs.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
