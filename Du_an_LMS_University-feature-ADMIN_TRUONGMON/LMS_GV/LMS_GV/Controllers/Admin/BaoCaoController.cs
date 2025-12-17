using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/bao-cao")]
    [Authorize(Roles = "Admin")]
    public class BaoCaoController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BaoCaoController(AppDbContext db)
        {
            _db = db;
        }

        public class CreateBaoCaoRequest
        {
            [Required]
            public int LoaiBaoCaoId { get; set; }

            [Required]
            public string FilePath { get; set; } = string.Empty;

            public int? CreatedBy { get; set; }
        }

        public class ExportRequest
        {
            [Required]
            public string Type { get; set; } = "csv"; // csv|pdf

            public int? Year { get; set; }
            public string? Filters { get; set; } // tuỳ, có thể là JSON
        }

        // 1. GET /
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var list = await (
                from bc in _db.BaoCaos.AsNoTracking()
                join lb in _db.LoaiBaoCaos.AsNoTracking()
                    on bc.LoaiBaoCaoId equals lb.LoaiBaoCaoId into glb
                from lb in glb.DefaultIfEmpty()
                join nd in _db.NguoiDungs.AsNoTracking()
                    on bc.CreatedBy equals nd.NguoiDungId into gnd
                from nd in gnd.DefaultIfEmpty()
                orderby bc.NgayTao descending
                select new
                {
                    id = bc.BaoCaoId,
                    loaiBaoCaoId = bc.LoaiBaoCaoId,
                    loaiBaoCao = lb != null ? lb.TenLoai : null,
                    filePath = bc.FilePath,
                    createdBy = nd != null ? nd.HoTen : null,
                    ngayTao = bc.NgayTao
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBaoCaoRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loaiExists = await _db.LoaiBaoCaos
                .AnyAsync(l => l.LoaiBaoCaoId == req.LoaiBaoCaoId);
            if (!loaiExists)
                return BadRequest(new { field = "loaiBaoCaoId", message = "Loại báo cáo không tồn tại" });

            if (req.CreatedBy.HasValue)
            {
                var ndExists = await _db.NguoiDungs
                    .AnyAsync(n => n.NguoiDungId == req.CreatedBy.Value);
                if (!ndExists)
                    return BadRequest(new { field = "createdBy", message = "Người tạo không tồn tại" });
            }

            var bc = new BaoCao
            {
                LoaiBaoCaoId = req.LoaiBaoCaoId,
                FilePath = req.FilePath,
                CreatedBy = req.CreatedBy,
                NgayTao = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.BaoCaos.Add(bc);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetList), new { }, new
            {
                id = bc.BaoCaoId
            });
        }

        // 3. POST /export – skeleton
        [HttpPost("export")]
        public IActionResult Export([FromBody] ExportRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ở đây bạn sẽ:
            // - build query theo year/filters
            // - tạo file Excel/PDF
            // - lưu file vào FilePath
            // - return link tải
            return StatusCode(501, new
            {
                message = "API export mới là skeleton, cần implement thêm logic sinh file thực tế."
            });
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bc = await _db.BaoCaos
                .FirstOrDefaultAsync(x => x.BaoCaoId == id);
            if (bc == null)
                return NotFound(new { message = "Không tìm thấy báo cáo" });

            _db.BaoCaos.Remove(bc);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
