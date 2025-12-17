using System.Linq;
using System.Threading.Tasks;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/ket-qua-kiem-tra")]
    [Authorize(Roles = "Admin")]
    public class KetQuaKiemTraController : ControllerBase
    {
        private readonly AppDbContext _db;

        public KetQuaKiemTraController(AppDbContext db)
        {
            _db = db;
        }

        public class KqListQuery
        {
            public int? BaiKiemTraId { get; set; }
        }

        // 1. GET /?baiKiemTraId=
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] KqListQuery queryModel)
        {
            var query = _db.KetQuaKiemTras.AsNoTracking().AsQueryable();

            if (queryModel.BaiKiemTraId.HasValue)
            {
                var id = queryModel.BaiKiemTraId.Value;
                query = query.Where(x => x.BaiKiemTraId == id);
            }

            var list = await (
                from kq in query
                join sv in _db.HoSoSinhViens on kq.SinhVienId equals sv.SinhVienId
                join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                select new
                {
                    id = kq.KetQuaKiemTraId,
                    baiKiemTraId = kq.BaiKiemTraId,
                    sinhVienId = kq.SinhVienId,
                    mssv = sv.Mssv,
                    fullName = nd.HoTen,
                    tongDiem = kq.TongDiem,
                    thoiGianLamBai = kq.ThoiGianLamBai,
                    isAutoGraded = kq.IsAutoGraded,
                    createdAt = kq.CreatedAt
                }
            ).ToListAsync();

            return Ok(list);
        }
    }
}
