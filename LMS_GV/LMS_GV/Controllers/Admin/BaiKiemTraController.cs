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
    [Route("api/admin/bai-kiem-tra")]
    [Authorize(Roles = "Admin")]
    public class BaiKiemTraController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BaiKiemTraController(AppDbContext db)
        {
            _db = db;
        }

        public class BaiKiemTraListQuery
        {
            public int? LopHocId { get; set; }
            public int? MonHocId { get; set; }
            public string? Loai { get; set; } // Bài tập/Bài kiểm tra/Bài thi...
        }

        public class CreateBaiKiemTraRequest
        {
            [Required]
            public int LopHocId { get; set; }

            public int? MonHocId { get; set; }
            public int? GiangVienId { get; set; }

            [Required]
            public string Loai { get; set; } = string.Empty;

            [Required]
            public string TieuDe { get; set; } = string.Empty;

            public string? MoTa { get; set; }

            public DateTime? NgayBatDau { get; set; }
            public DateTime? NgayKetThuc { get; set; }
            public int? ThoiGianLamBai { get; set; }
            public int? SoCau { get; set; }
            public decimal? DiemToiDa { get; set; }
            public bool? IsRandom { get; set; }
            public bool? ChoPhepLamLai { get; set; }
            public int? SoLanLamToiDa { get; set; }
        }

        public class UpdateBaiKiemTraRequest : CreateBaiKiemTraRequest
        {
        }

        // 1. GET / – list
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] BaiKiemTraListQuery queryModel)
        {
            var query = _db.BaiKiemTras.AsNoTracking().AsQueryable();

            if (queryModel.LopHocId.HasValue)
            {
                var id = queryModel.LopHocId.Value;
                query = query.Where(x => x.LopHocId == id);
            }

            if (queryModel.MonHocId.HasValue)
            {
                var id = queryModel.MonHocId.Value;
                query = query.Where(x => x.MonHocId == id);
            }

            if (!string.IsNullOrWhiteSpace(queryModel.Loai))
            {
                var loai = queryModel.Loai.Trim();
                query = query.Where(x => x.Loai == loai);
            }

            var list = await (
                from bkt in query
                join l in _db.LopHocs on bkt.LopHocId equals l.LopHocId
                join m in _db.MonHocs on bkt.MonHocId equals m.MonHocId into gm
                from m in gm.DefaultIfEmpty()
                join ng in _db.Nganhs on l.NganhId equals ng.NganhId into gng
                from ng in gng.DefaultIfEmpty()
                select new
                {
                    id = bkt.BaiKiemTraId,
                    tieuDe = bkt.TieuDe,
                    loai = bkt.Loai,
                    lopHocId = bkt.LopHocId,
                    maLop = l.MaLop,
                    monHocId = bkt.MonHocId ?? l.MonHocId,
                    tenMon = m != null ? m.TenMon : null,
                    nganh = ng != null ? ng.TenNganh : null,
                    ngayNop = bkt.NgayKetThuc
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. GET /{id} – detail
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var bkt = await _db.BaiKiemTras
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BaiKiemTraId == id);
            if (bkt == null)
                return NotFound(new { message = "Không tìm thấy bài kiểm tra" });

            var lop = await _db.LopHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LopHocId == bkt.LopHocId);

            var mon = await _db.MonHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MonHocId == (bkt.MonHocId ?? lop.MonHocId));

            var nganh = await _db.Nganhs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NganhId == lop.NganhId);

            var gvInfo = await (
                from gv in _db.GiangViens
                join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                where gv.GiangVienId == (bkt.GiangVienId ?? lop.GiangVienId)
                select new
                {
                    name = nd.HoTen,
                    email = nd.Email
                }
            ).FirstOrDefaultAsync();

            var result = new
            {
                id = bkt.BaiKiemTraId,
                tieuDe = bkt.TieuDe,
                loai = bkt.Loai,
                lopHocId = bkt.LopHocId,
                maLop = lop?.MaLop,
                monHocId = mon?.MonHocId,
                tenMon = mon?.TenMon,
                nganh = nganh?.TenNganh,
                giangVien = gvInfo,
                ngayDang = bkt.CreatedAt,
                ngayNop = bkt.NgayKetThuc,
                moTa = bkt.MoTa,
                thoiGianLamBai = bkt.ThoiGianLamBai,
                soCau = bkt.SoCau,
                diemToiDa = bkt.DiemToiDa
            };

            return Ok(result);
        }

        // 3. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBaiKiemTraRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lop = await _db.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == req.LopHocId);
            if (lop == null)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            if (req.MonHocId.HasValue)
            {
                var existsMon = await _db.MonHocs.AnyAsync(m => m.MonHocId == req.MonHocId.Value);
                if (!existsMon)
                    return BadRequest(new { field = "monHocId", message = "Môn học không tồn tại" });
            }

            if (req.GiangVienId.HasValue)
            {
                var existsGv = await _db.GiangViens.AnyAsync(gv => gv.GiangVienId == req.GiangVienId.Value);
                if (!existsGv)
                    return BadRequest(new { field = "giangVienId", message = "Giảng viên không tồn tại" });
            }

            var entity = new BaiKiemTra
            {
                LopHocId = req.LopHocId,
                MonHocId = req.MonHocId ?? lop.MonHocId,
                GiangVienId = req.GiangVienId,
                Loai = req.Loai,
                TieuDe = req.TieuDe,
                MoTa = req.MoTa,
                NgayBatDau = req.NgayBatDau,
                NgayKetThuc = req.NgayKetThuc,
                ThoiGianLamBai = req.ThoiGianLamBai,
                SoCau = req.SoCau,
                DiemToiDa = req.DiemToiDa,
                IsRandom = req.IsRandom ?? false,
                ChoPhepLamLai = req.ChoPhepLamLai ?? false,
                SoLanLamToiDa = req.SoLanLamToiDa,
                TrangThai = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = req.GiangVienId
            };

            _db.BaiKiemTras.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetail), new { id = entity.BaiKiemTraId }, new
            {
                id = entity.BaiKiemTraId
            });
        }

        // 4. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBaiKiemTraRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.BaiKiemTras
                .FirstOrDefaultAsync(x => x.BaiKiemTraId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy bài kiểm tra" });

            var lop = await _db.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == req.LopHocId);
            if (lop == null)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            if (req.MonHocId.HasValue)
            {
                var existsMon = await _db.MonHocs.AnyAsync(m => m.MonHocId == req.MonHocId.Value);
                if (!existsMon)
                    return BadRequest(new { field = "monHocId", message = "Môn học không tồn tại" });
            }

            if (req.GiangVienId.HasValue)
            {
                var existsGv = await _db.GiangViens.AnyAsync(gv => gv.GiangVienId == req.GiangVienId.Value);
                if (!existsGv)
                    return BadRequest(new { field = "giangVienId", message = "Giảng viên không tồn tại" });
            }

            entity.LopHocId = req.LopHocId;
            entity.MonHocId = req.MonHocId ?? lop.MonHocId;
            entity.GiangVienId = req.GiangVienId;
            entity.Loai = req.Loai;
            entity.TieuDe = req.TieuDe;
            entity.MoTa = req.MoTa;
            entity.NgayBatDau = req.NgayBatDau;
            entity.NgayKetThuc = req.NgayKetThuc;
            entity.ThoiGianLamBai = req.ThoiGianLamBai;
            entity.SoCau = req.SoCau;
            entity.DiemToiDa = req.DiemToiDa;
            entity.IsRandom = req.IsRandom ?? entity.IsRandom;
            entity.ChoPhepLamLai = req.ChoPhepLamLai ?? entity.ChoPhepLamLai;
            entity.SoLanLamToiDa = req.SoLanLamToiDa;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 5. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.BaiKiemTras
                .FirstOrDefaultAsync(x => x.BaiKiemTraId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy bài kiểm tra" });

            var hasCauHoi = await _db.CauHois.AnyAsync(c => c.BaiKiemTraId == id);
            var hasBaiNop = await _db.BaiNops.AnyAsync(b => b.BaiKiemTraId == id);
            var hasKQ = await _db.KetQuaKiemTras.AnyAsync(k => k.BaiKiemTraId == id);

            if (hasCauHoi || hasBaiNop || hasKQ)
            {
                return BadRequest(new { message = "Không thể xóa vì đề đã có dữ liệu liên quan (câu hỏi/bài nộp/kết quả)" });
            }

            _db.BaiKiemTras.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
