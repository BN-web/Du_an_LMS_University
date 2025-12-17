using System.Linq;
using System.Threading.Tasks;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/thong-ke/hoc-tap")]
    [Authorize(Roles = "Admin")]
    public class ThongKeHocTapController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ThongKeHocTapController(AppDbContext db)
        {
            _db = db;
        }

        // 1. GET / – dữ liệu biểu đồ: SV theo khóa, GV theo chuyên môn/ngành (tương đương)
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Tổng quan
            var totalStudents = await _db.HoSoSinhViens.CountAsync();
            var totalTeachers = await _db.GiangViens.CountAsync();
            var totalMajors = await _db.Nganhs.CountAsync();
            var totalFaculties = await _db.Khoas.CountAsync();

            // Sinh viên theo khóa (bar)
            var studentsByCohort = await (
                from k in _db.KhoaTuyenSinhs.AsNoTracking()
                select new
                {
                    cohortId = k.KhoaTuyenSinhId,
                    cohortCode = k.MaKhoaTuyenSinh,
                    cohortName = k.TenKhoaTuyenSinh,
                    totalStudents = _db.HoSoSinhViens
                        .Count(sv => sv.KhoaTuyenSinhId == k.KhoaTuyenSinhId)
                }
            ).ToListAsync();

            // Giảng viên theo "ngành" – ở schema hiện tại chỉ có Khoa + ChuyenMon,
            // nên mình group theo ChuyenMon như ngành/chuyên ngành.
            var teacherByMajor = await (
                from gl in _db.GiangVienLops
                join l in _db.LopHocs on gl.LopHocId equals l.LopHocId
                join ng in _db.Nganhs on l.NganhId equals ng.NganhId
                group gl by new { ng.NganhId, ng.TenNganh } into g
                select new
                {
                    label = g.Key.TenNganh,
                    total = g.Select(x => x.GiangVienId).Distinct().Count()
                }
            ).ToListAsync();

            return Ok(new
            {
                summary = new
                {
                    totalStudents,
                    totalTeachers,
                    totalMajors,
                    totalFaculties
                },
                studentsByCohort,
                teachersByMajor = teacherByMajor
            });
        }
    }
}
