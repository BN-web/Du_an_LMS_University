using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LMS_GV.Models;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/files")]
    [Authorize(Roles = "Admin")]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public FilesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public class FileListQuery
        {
            public int? BaiHocId { get; set; }
            public string? Q { get; set; }
        }

        // 1. GET / – list
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] FileListQuery queryModel)
        {
            var query = _db.Files
                .AsNoTracking()
                .AsQueryable();

            if (queryModel.BaiHocId.HasValue)
            {
                var id = queryModel.BaiHocId.Value;
                query = query.Where(f => f.BaiHocId == id);
            }

            if (!string.IsNullOrWhiteSpace(queryModel.Q))
            {
                var kw = queryModel.Q.Trim();
                query = query.Where(f =>
                    (f.TenFile ?? "").Contains(kw) ||
                    (f.DuongDan ?? "").Contains(kw));
            }

            var list = await (
                from f in query
                join b in _db.BaiHocs on f.BaiHocId equals b.BaiHocId into gb
                from b in gb.DefaultIfEmpty()
                join nd in _db.NguoiDungs on b.CreatedBy equals nd.NguoiDungId into gnd
                from nd in gnd.DefaultIfEmpty()
                select new
                {
                    id = f.FilesId,
                    tenFile = f.TenFile,
                    link = f.DuongDan,
                    baiHocId = f.BaiHocId,
                    tieuDe = b != null ? b.TieuDe : null,
                    loaiBaiHoc = b != null ? b.LoaiBaiHoc : null,
                    nguoiUpload = nd != null ? nd.HoTen : null,
                    monHoc = (
                        (from bl in _db.BaiHocLops
                         join l in _db.LopHocs on bl.LopHocId equals l.LopHocId
                         join m in _db.MonHocs on l.MonHocId equals m.MonHocId
                         where bl.BaiHocId == f.BaiHocId
                         select m.TenMon).Distinct().FirstOrDefault()
                    ),
                    createdAt = f.CreatedAt
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. POST /upload – upload file
        [HttpPost("upload")]
        [RequestSizeLimit(100_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] int? baiHocId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File trống" });

            if (baiHocId.HasValue)
            {
                var existsBaiHoc = await _db.BaiHocs.AnyAsync(b => b.BaiHocId == baiHocId.Value);
                if (!existsBaiHoc)
                    return BadRequest(new { field = "baiHocId", message = "Bài học không tồn tại" });
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var safeFileName = Path.GetFileName(file.FileName);
            var destPath = Path.Combine(uploadsFolder, safeFileName);

            using (var stream = new FileStream(destPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relPath = "/uploads/" + safeFileName;

            var entity = new LMS_GV.Models.File
            {
                BaiHocId = baiHocId,
                TenFile = safeFileName,
                DuongDan = relPath,
                KichThuoc = file.Length,
                CreatedAt = DateTime.UtcNow
            };

            _db.Files.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                id = entity.FilesId,
                fileName = entity.TenFile,
                url = entity.DuongDan
            });
        }

        // 3. GET /{id}/download
        [HttpGet("{id:int}/download")]
        [AllowAnonymous] // tuỳ, nếu muốn cho SV tải không cần Admin
        public async Task<IActionResult> Download(int id)
        {
            var file = await _db.Files.AsNoTracking()
                .FirstOrDefaultAsync(f => f.FilesId == id);
            if (file == null)
                return NotFound(new { message = "Không tìm thấy file" });

            var path = Path.Combine(_env.WebRootPath ?? "wwwroot",
                (file.DuongDan ?? "").TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!System.IO.File.Exists(path))
                return NotFound(new { message = "File vật lý không tồn tại trên server" });

            var contentType = "application/octet-stream";
            return PhysicalFile(path, contentType, file.TenFile ?? Path.GetFileName(path));
        }

        // 4. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var file = await _db.Files
                .FirstOrDefaultAsync(f => f.FilesId == id);
            if (file == null)
                return NotFound(new { message = "Không tìm thấy file" });

            var path = Path.Combine(_env.WebRootPath ?? "wwwroot",
                (file.DuongDan ?? "").TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            _db.Files.Remove(file);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
