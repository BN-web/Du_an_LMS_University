using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LMS_GV.Models.Data;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        // ========== 1. GET /api/admin/dashboard/stats ==========
        // Tổng số ngành, khoa, giảng viên, sinh viên
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalDepartments = await _db.Khoas.CountAsync();          // Khoa
            var totalMajors      = await _db.Nganhs.CountAsync();         // Ngành
            var totalTeachers    = await _db.GiangViens.CountAsync();     // Giảng viên
            var totalStudents    = await _db.HoSoSinhViens.CountAsync();  // Sinh viên

            return Ok(new
            {
                departments = totalDepartments,
                majors      = totalMajors,
                teachers    = totalTeachers,
                students    = totalStudents
            });
        }

        // ========== 2. GET /api/admin/dashboard/cohorts ==========
        // Biểu đồ cột: số lượng sinh viên theo khóa tuyển sinh (KhoaTuyenSinh)
        [HttpGet("cohorts")]
        public async Task<IActionResult> GetCohortsStats()
        {
            // Group sinh viên theo khóa tuyển sinh
            var data = await _db.HoSoSinhViens
                .AsNoTracking()
                .Where(sv => sv.KhoaTuyenSinhId != null)
                .GroupBy(sv => sv.KhoaTuyenSinhId)
                .Select(g => new { cohortId = g.Key.Value, studentCount = g.Count() })
                .Join(_db.KhoaTuyenSinhs.AsNoTracking(),
                      g => g.cohortId,
                      kts => kts.KhoaTuyenSinhId,
                      (g, kts) => new {
                          cohortId = g.cohortId,
                          code = kts.MaKhoaTuyenSinh,
                          name = kts.TenKhoaTuyenSinh,
                          studentCount = g.studentCount
                      })
                .OrderByDescending(x => x.studentCount)
                .ToListAsync();

            return Ok(data);
        }

        // ========== 3. GET /api/admin/dashboard/teachers-by-major ==========
        // Biểu đồ tròn/miền: giảng viên theo Khoa (gần với "ngành/chuyên môn")
        [HttpGet("teachers-by-major")]
        public async Task<IActionResult> GetTeachersByMajor()
        {
            // Join GiangVien với Khoa qua KhoaId
            var data = await _db.GiangViens
                .AsNoTracking()
                .Where(gv => gv.KhoaId != null)
                .GroupBy(gv => gv.KhoaId)
                .Select(g => new { facultyId = g.Key.Value, teacherCount = g.Count() })
                .Join(_db.Khoas.AsNoTracking(),
                      g => g.facultyId,
                      k => k.KhoaId,
                      (g, k) => new {
                          facultyId = g.facultyId,
                          facultyName = k.TenKhoa,
                          teacherCount = g.teacherCount
                      })
                .OrderByDescending(x => x.teacherCount)
                .ToListAsync();

            // Nếu bạn muốn group theo ChuyenMon thay vì Khoa:
            // var data = await _db.GiangViens
            //     .GroupBy(gv => gv.ChuyenMon)
            //     .Select(g => new {
            //         specialization = g.Key,
            //         teacherCount = g.Count()
            //     }).ToListAsync();

            return Ok(data);
        }

        // ========== 4. GET /api/admin/dashboard/recent-activities ==========
        // Nhật ký hệ thống gần đây
        [HttpGet("recent-activities")]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 10)
        {
            if (limit <= 0 || limit > 50) limit = 10; // tránh request quá lớn

            var activities = await _db.NhatKyHeThongs
                .Include(x => x.NguoiDung)
                .OrderByDescending(x => x.CreatedAt)  // cột created_at
                .Take(limit)
                .Select(x => new
                {
                    id          = x.NhatKyHeThongId,
                    userId      = x.NguoiDungId,
                    username    = x.NguoiDung != null ? x.NguoiDung.TenDangNhap : null,
                    fullName    = x.NguoiDung != null ? x.NguoiDung.HoTen : null,
                    action      = x.HanhDong,
                    reference   = x.ThamChieu,
                    createdAt   = x.CreatedAt
                })
                .ToListAsync();

            return Ok(activities);
        }
    }
}
