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
    [Route("api/admin/nganh")]
    [Authorize(Roles = "Admin")]
    public class NganhController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NganhController(AppDbContext db)
        {
            _db = db;
        }

        // ========== DTOs ==========

        public class NganhListQuery
        {
            public string? Search { get; set; }
            public int? KhoaId { get; set; }
        }

        public class CreateNganhRequest
        {
            [Required]
            [StringLength(10, ErrorMessage = "Mã ngành tối đa 10 ký tự")]
            public string MaNganh { get; set; } = string.Empty;

            [Required]
            public string TenNganh { get; set; } = string.Empty;

            [Required]
            public int KhoaId { get; set; }

            // Chỉ dùng để hiển thị logic, không lưu DB vì không có cột
            public string? TenTruongKhoa { get; set; }
        }

        public class UpdateNganhRequest
        {
            [Required]
            [StringLength(10, ErrorMessage = "Mã ngành tối đa 10 ký tự")]
            public string MaNganh { get; set; } = string.Empty;

            [Required]
            public string TenNganh { get; set; } = string.Empty;

            [Required]
            public int KhoaId { get; set; }

            public string? TenTruongKhoa { get; set; }
        }

        // ========== 1. GET /  – list + search + filter ==========

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] NganhListQuery queryModel)
        {
            var query = _db.Nganhs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var kw = queryModel.Search.Trim();
                query = query.Where(n =>
                    (n.MaNganh ?? "").Contains(kw) ||
                    (n.TenNganh ?? "").Contains(kw));
            }

            if (queryModel.KhoaId.HasValue)
            {
                var kId = queryModel.KhoaId.Value;
                query = query.Where(n => n.KhoaId == kId);
            }

            var list = await query
                .OrderBy(n => n.TenNganh)
                .Select(n => new
                {
                    id = n.NganhId,
                    code = n.MaNganh,
                    name = n.TenNganh,
                    khoaId = n.KhoaId,
                    khoaName = _db.Khoas
                        .Where(k => k.KhoaId == n.KhoaId)
                        .Select(k => k.TenKhoa)
                        .FirstOrDefault(),
                    totalStudents = _db.HoSoSinhViens
                        .Count(sv => sv.NganhId == n.NganhId),

                    // Số môn trong ngành dựa trên CTĐT
                    totalSubjects =
                        (from ct in _db.ChuongTrinhDaoTaos
                         join ctmh in _db.ChuongTrinhMonHocs
                             on ct.ChuongTrinhDaoTaoId equals ctmh.ChuongTrinhDaoTaoId
                         where ct.NganhId == n.NganhId
                         select ctmh.MonHocId)
                        .Distinct()
                        .Count(),

                    createdAt = n.CreatedAt,
                    updatedAt = n.UpdatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // ========== 2. GET /{id} – detail ==========

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var nganh = await _db.Nganhs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NganhId == id);

            if (nganh == null)
                return NotFound(new { message = "Không tìm thấy ngành học" });

            var khoa = await _db.Khoas
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.KhoaId == nganh.KhoaId);

            // Trưởng khoa: lấy từ GiangVien có ChucVu chứa "Trưởng khoa"
            var truongKhoaName = await (
                from gv in _db.GiangViens
                join nd in _db.NguoiDungs
                    on gv.NguoiDungId equals nd.NguoiDungId
                where gv.KhoaId == nganh.KhoaId
                      && gv.ChucVu != null
                      && gv.ChucVu.Contains("Trưởng khoa")
                select nd.HoTen
            ).FirstOrDefaultAsync();

            var totalStudents = await _db.HoSoSinhViens
                .CountAsync(sv => sv.NganhId == nganh.NganhId);

            // Giảng viên chuyên ngành: tạm tính theo Khoa của ngành
            var totalLecturers = await _db.GiangViens
                .CountAsync(gv => gv.KhoaId == nganh.KhoaId);

            var result = new
            {
                id = nganh.NganhId,
                code = nganh.MaNganh,
                name = nganh.TenNganh,
                khoaId = nganh.KhoaId,
                khoaName = khoa?.TenKhoa,
                truongKhoa = truongKhoaName,
                totalStudents,
                totalLecturers,
                createdAt = nganh.CreatedAt,
                updatedAt = nganh.UpdatedAt
            };

            return Ok(result);
        }

        // ========== 3. POST / – create ==========

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNganhRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (req.MaNganh.Length > 10)
            {
                return BadRequest(new { message = "Mã ngành tối đa 10 ký tự" });
            }

            var existsCode = await _db.Nganhs
                .AnyAsync(n => n.MaNganh == req.MaNganh);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "maNganh",
                    message = "Mã ngành đã tồn tại"
                });
            }

            var existsKhoa = await _db.Khoas
                .AnyAsync(k => k.KhoaId == req.KhoaId);
            if (!existsKhoa)
            {
                return BadRequest(new { field = "khoaId", message = "Khoa không tồn tại" });
            }

            var nganh = new Nganh
            {
                MaNganh = req.MaNganh,
                TenNganh = req.TenNganh,
                KhoaId = req.KhoaId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Nganhs.Add(nganh);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetail), new { id = nganh.NganhId }, new
            {
                id = nganh.NganhId,
                code = nganh.MaNganh
            });
        }

        // ========== 4. PUT /{id} – update ==========

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNganhRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nganh = await _db.Nganhs
                .FirstOrDefaultAsync(n => n.NganhId == id);

            if (nganh == null)
                return NotFound(new { message = "Không tìm thấy ngành học" });

            if (req.MaNganh.Length > 10)
                return BadRequest(new { message = "Mã ngành tối đa 10 ký tự" });

            // Check mã trùng
            var existsCode = await _db.Nganhs
                .AnyAsync(n => n.MaNganh == req.MaNganh && n.NganhId != id);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "maNganh",
                    message = "Mã ngành đã tồn tại"
                });
            }

            var existsKhoa = await _db.Khoas
                .AnyAsync(k => k.KhoaId == req.KhoaId);
            if (!existsKhoa)
            {
                return BadRequest(new { field = "khoaId", message = "Khoa không tồn tại" });
            }

            nganh.MaNganh = req.MaNganh;
            nganh.TenNganh = req.TenNganh;
            nganh.KhoaId = req.KhoaId;
            nganh.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========== 5. DELETE /{id} – delete ==========

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var nganh = await _db.Nganhs
                .FirstOrDefaultAsync(n => n.NganhId == id);

            if (nganh == null)
                return NotFound(new { message = "Không tìm thấy ngành học" });

            var hasStudents = await _db.HoSoSinhViens
                .AnyAsync(sv => sv.NganhId == id);
            if (hasStudents)
            {
                return BadRequest(new
                {
                    message = "Không thể xóa ngành vì đang có sinh viên thuộc ngành này"
                });
            }

            var hasPrograms = await _db.ChuongTrinhDaoTaos
                .AnyAsync(ct => ct.NganhId == id);
            if (hasPrograms)
            {
                return BadRequest(new
                {
                    message = "Không thể xóa ngành vì đang có chương trình đào tạo sử dụng"
                });
            }

            _db.Nganhs.Remove(nganh);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
