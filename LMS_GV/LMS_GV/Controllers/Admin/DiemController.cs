using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
    [Route("api/admin/diem")]
    [Authorize(Roles = "Admin")]
    public class DiemController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DiemController(AppDbContext db)
        {
            _db = db;
        }

        public class DiemListQuery
        {
            public int? SinhVienId { get; set; }
            public int? LopHocId { get; set; }
            public int? HocKyId { get; set; }
            public int? KhoaId { get; set; }
            public int? NganhId { get; set; }
            public int? MonHocId { get; set; }
            public string? Q { get; set; } // mssv / tên
        }

        public class CreateDiemRequest
        {
            [Required]
            public int SinhVienId { get; set; }

            [Required]
            public int LopHocId { get; set; }

            [Required]
            public int HocKyId { get; set; }

            public decimal? DiemTrungBinhMon { get; set; }
            public string? DiemChu { get; set; }
            public decimal? GpaMon { get; set; }
            public decimal? HeSoMon { get; set; }
            public int? SoTinChi { get; set; }
            public byte? TrangThai { get; set; } // 1=Đậu,0=Rớt
            public string? LyDoRoiMon { get; set; }
        }

        public class UpdateDiemRequest : CreateDiemRequest
        {
        }

        // 1. GET / – list
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] DiemListQuery queryModel)
        {
            var query = _db.Diems.AsNoTracking().AsQueryable();

            if (queryModel.SinhVienId.HasValue)
            {
                var id = queryModel.SinhVienId.Value;
                query = query.Where(x => x.SinhVienId == id);
            }

            if (queryModel.LopHocId.HasValue)
            {
                var id = queryModel.LopHocId.Value;
                query = query.Where(x => x.LopHocId == id);
            }

            if (queryModel.HocKyId.HasValue)
            {
                var id = queryModel.HocKyId.Value;
                query = query.Where(x => x.HocKyId == id);
            }

            // join để filter theo Khoa/Nganh/MonHoc + q
            var resultQuery =
                from d in query
                join sv in _db.HoSoSinhViens on d.SinhVienId equals sv.SinhVienId
                join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                join l in _db.LopHocs on d.LopHocId equals l.LopHocId
                join m in _db.MonHocs on l.MonHocId equals m.MonHocId
                join ng in _db.Nganhs on l.NganhId equals ng.NganhId into gng
                from ng in gng.DefaultIfEmpty()
                join k in _db.Khoas on ng.KhoaId equals k.KhoaId into gk
                from k in gk.DefaultIfEmpty()
                select new
                {
                    d,
                    sv,
                    nd,
                    l,
                    m,
                    ng,
                    k
                };

            if (queryModel.KhoaId.HasValue)
            {
                var id = queryModel.KhoaId.Value;
                resultQuery = resultQuery.Where(x => x.k != null && x.k.KhoaId == id);
            }

            if (queryModel.NganhId.HasValue)
            {
                var id = queryModel.NganhId.Value;
                resultQuery = resultQuery.Where(x => x.ng != null && x.ng.NganhId == id);
            }

            if (queryModel.MonHocId.HasValue)
            {
                var id = queryModel.MonHocId.Value;
                resultQuery = resultQuery.Where(x => x.m.MonHocId == id);
            }

            if (!string.IsNullOrWhiteSpace(queryModel.Q))
            {
                var kw = queryModel.Q.Trim().ToLower(CultureInfo.GetCultureInfo("vi-VN"));
                resultQuery = resultQuery.Where(x =>
                    (x.sv.Mssv ?? "").ToLower().Contains(kw) ||
                    (x.nd.HoTen ?? "").ToLower().Contains(kw));
            }

            var list = await resultQuery
                .OrderByDescending(x => x.d.CreatedAt)
                .Select(x => new
                {
                    id = x.d.DiemId,
                    sinhVienId = x.sv.SinhVienId,
                    mssv = x.sv.Mssv,
                    fullName = x.nd.HoTen,
                    nganh = x.ng != null ? x.ng.TenNganh : null,
                    monHoc = x.m.TenMon,
                    soTinChi = x.d.SoTinChi ?? x.m.SoTinChi,
                    diemTrungBinhMon = x.d.DiemTrungBinhMon,
                    diemChu = x.d.DiemChu
                })
                .ToListAsync();

            return Ok(list);
        }

        // 2. GET /{id} – detail
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var d = await _db.Diems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DiemId == id);
            if (d == null)
                return NotFound(new { message = "Không tìm thấy bản ghi điểm" });

            var sv = await _db.HoSoSinhViens.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SinhVienId == d.SinhVienId);
            var nd = sv != null
                ? await _db.NguoiDungs.AsNoTracking()
                    .FirstOrDefaultAsync(n => n.NguoiDungId == sv.NguoiDungId)
                : null;

            var lop = await _db.LopHocs.AsNoTracking()
                .FirstOrDefaultAsync(l => l.LopHocId == d.LopHocId);

            var mon = lop != null
                ? await _db.MonHocs.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MonHocId == lop.MonHocId)
                : null;

            var ng = lop != null && lop.NganhId.HasValue
                ? await _db.Nganhs.AsNoTracking()
                    .FirstOrDefaultAsync(n => n.NganhId == lop.NganhId.Value)
                : null;

            // điểm thành phần
            var thanhPhan = await (
                from tp in _db.ThanhPhanDiems.AsNoTracking()
                join dtp in _db.DiemThanhPhans.AsNoTracking()
                    on new { tp.ThanhPhanDiemId, d.SinhVienId, d.LopHocId }
                    equals new { dtp.ThanhPhanDiemId, dtp.SinhVienId, dtp.LopHocId } into gdtp
                from dtp in gdtp.DefaultIfEmpty()
                where tp.LopHocId == d.LopHocId
                select new
                {
                    tp.Ten,
                    tp.HeSo,
                    Diem = dtp != null ? dtp.Diem : (decimal?)null
                }
            ).ToListAsync();

            decimal? diemBaiTap = thanhPhan
                .FirstOrDefault(x => (x.Ten ?? "").ToLower().Contains("bài tập"))?.Diem;
            decimal? diemChuyenCan = thanhPhan
                .FirstOrDefault(x => (x.Ten ?? "").ToLower().Contains("chuyên cần"))?.Diem;
            decimal? diemGiuaKy = thanhPhan
                .FirstOrDefault(x => (x.Ten ?? "").ToLower().Contains("giữa"))?.Diem;
            decimal? diemCuoiKy = thanhPhan
                .FirstOrDefault(x => (x.Ten ?? "").ToLower().Contains("cuối"))?.Diem;

            var result = new
            {
                id = d.DiemId,
                sinhVienId = d.SinhVienId,
                mssv = sv?.Mssv,
                fullName = nd?.HoTen,
                nganh = ng?.TenNganh,
                monHoc = mon?.TenMon,
                soTinChi = d.SoTinChi ?? mon?.SoTinChi,
                diemTrungBinhMon = d.DiemTrungBinhMon,
                diemChu = d.DiemChu,
                gpaMon = d.Gpamon,
                diemBaiTap,
                diemChuyenCan,
                diemGiuaKy,
                diemCuoiKy,
                trangThai = d.TrangThai,
                lyDoRoiMon = d.LyDoRoiMon
            };

            return Ok(result);
        }

        // 3. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDiemRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var svExists = await _db.HoSoSinhViens
                .AnyAsync(s => s.SinhVienId == req.SinhVienId);
            if (!svExists)
                return BadRequest(new { field = "sinhVienId", message = "Sinh viên không tồn tại" });

            var lop = await _db.LopHocs
                .FirstOrDefaultAsync(l => l.LopHocId == req.LopHocId);
            if (lop == null)
                return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });

            var hkExists = await _db.HocKys.AnyAsync(h => h.HocKyId == req.HocKyId);
            if (!hkExists)
                return BadRequest(new { field = "hocKyId", message = "Học kỳ không tồn tại" });

            var dup = await _db.Diems
                .AnyAsync(x => x.SinhVienId == req.SinhVienId
                               && x.LopHocId == req.LopHocId
                               && x.HocKyId == req.HocKyId);
            if (dup)
                return Conflict(new { message = "Đã tồn tại bản ghi điểm cho sinh viên này" });

            var entity = new Diem
            {
                SinhVienId = req.SinhVienId,
                LopHocId = req.LopHocId,
                HocKyId = req.HocKyId,
                DiemTrungBinhMon = req.DiemTrungBinhMon,
                DiemChu = req.DiemChu,
                Gpamon = req.GpaMon,
                HeSoMon = req.HeSoMon ?? 1,
                SoTinChi = req.SoTinChi ?? lop.SoTinChi,
                TrangThai = req.TrangThai ?? 1,
                LyDoRoiMon = req.LyDoRoiMon,
                CreatedAt = DateTime.UtcNow
            };

            _db.Diems.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetail), new { id = entity.DiemId }, new
            {
                id = entity.DiemId
            });
        }

        // 4. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDiemRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await _db.Diems
                .FirstOrDefaultAsync(x => x.DiemId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy bản ghi điểm" });

            if (req.SinhVienId != 0)
                entity.SinhVienId = req.SinhVienId;
            if (req.LopHocId != 0)
                entity.LopHocId = req.LopHocId;
            if (req.HocKyId != 0)
                entity.HocKyId = req.HocKyId;

            if (req.DiemTrungBinhMon.HasValue)
                entity.DiemTrungBinhMon = req.DiemTrungBinhMon.Value;
            if (!string.IsNullOrWhiteSpace(req.DiemChu))
                entity.DiemChu = req.DiemChu;
            if (req.GpaMon.HasValue)
                entity.Gpamon = req.GpaMon.Value;
            if (req.HeSoMon.HasValue)
                entity.HeSoMon = req.HeSoMon.Value;
            if (req.SoTinChi.HasValue)
                entity.SoTinChi = req.SoTinChi.Value;
            if (req.TrangThai.HasValue)
                entity.TrangThai = req.TrangThai.Value;
            if (!string.IsNullOrWhiteSpace(req.LyDoRoiMon))
                entity.LyDoRoiMon = req.LyDoRoiMon;

            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 5. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Diems
                .FirstOrDefaultAsync(x => x.DiemId == id);
            if (entity == null)
                return NotFound(new { message = "Không tìm thấy bản ghi điểm" });

            _db.Diems.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
