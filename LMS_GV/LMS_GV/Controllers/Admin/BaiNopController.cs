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
    [Route("api/admin/bai-nop")]
    [Authorize(Roles = "Admin")]
    public class BaiNopController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BaiNopController(AppDbContext db)
        {
            _db = db;
        }

        public class BaiNopListQuery
        {
            public int? BaiKiemTraId { get; set; }
            public int? SinhVienId { get; set; }
        }

        public class UpdateBaiNopRequest
        {
            public decimal? TongDiem { get; set; }
            public byte? TrangThai { get; set; } // 0=gian lận,1=đã chấm
            public int? LanLam { get; set; }
            public int? ThoiGianHoanThanh { get; set; }
        }

        // 1. GET /?baiKiemTraId=&sinhVienId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] BaiNopListQuery queryModel)
        {
            var query = _db.BaiNops.AsNoTracking().AsQueryable();

            if (queryModel.BaiKiemTraId.HasValue)
            {
                var id = queryModel.BaiKiemTraId.Value;
                query = query.Where(x => x.BaiKiemTraId == id);
            }

            if (queryModel.SinhVienId.HasValue)
            {
                var id = queryModel.SinhVienId.Value;
                query = query.Where(x => x.SinhVienId == id);
            }

            var list = await (
                from bn in query
                join sv in _db.HoSoSinhViens on bn.SinhVienId equals sv.SinhVienId
                join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                join bkt in _db.BaiKiemTras on bn.BaiKiemTraId equals bkt.BaiKiemTraId
                select new
                {
                    id = bn.BaiNopId,
                    baiKiemTraId = bn.BaiKiemTraId,
                    tieuDe = bkt.TieuDe,
                    sinhVienId = bn.SinhVienId,
                    mssv = sv.Mssv,
                    fullName = nd.HoTen,
                    ngayNop = bn.NgayNop,
                    tongDiem = bn.TongDiem,
                    trangThai = bn.TrangThai,
                    lanLam = bn.LanLam,
                    thoiGianHoanThanh = bn.ThoiGianHoanThanh
                }
            ).ToListAsync();

            return Ok(list);
        }

        // 2. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBaiNopRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bn = await _db.BaiNops
                .FirstOrDefaultAsync(b => b.BaiNopId == id);
            if (bn == null)
                return NotFound(new { message = "Không tìm thấy bài nộp" });

            if (req.TongDiem.HasValue) bn.TongDiem = req.TongDiem.Value;
            if (req.TrangThai.HasValue) bn.TrangThai = req.TrangThai.Value;
            if (req.LanLam.HasValue) bn.LanLam = req.LanLam.Value;
            if (req.ThoiGianHoanThanh.HasValue) bn.ThoiGianHoanThanh = req.ThoiGianHoanThanh.Value;

            bn.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 3. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bn = await _db.BaiNops
                .FirstOrDefaultAsync(b => b.BaiNopId == id);
            if (bn == null)
                return NotFound(new { message = "Không tìm thấy bài nộp" });

            // Có thể cần xoá ChiTietCauTraLoi nếu không cascade
            var details = _db.ChiTietCauTraLois
                .Where(c => c.BaiNopId == id);
            _db.ChiTietCauTraLois.RemoveRange(details);

            _db.BaiNops.Remove(bn);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
