using LMS_GV.Models;
using LMS_GV.Models.Data;
using LMS_GV.Models.DTO_GiangVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LMS_GV.Controllers_GiangVien
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Giảng Viên")]
    public class GV_DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GV_DashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary> Tổng quan dashboard 
        /// Lịch dạy hôm nay
        //Thống kê: tổng lớp, tổng sinh viên
        //Thông báo mới(dùng bảng ThongBao + ThongBaoNguoiDung)
        //Biểu đồ bài tập(dùng BaiKiemTra + BaiNop)
        //Hoạt động gần đây(dùng NhatKyHeThong)
        /// </summary>




        /// <summary>
        /// API: Lấy tổng quan Dashboard giảng viên
        /// Bao gồm: Lịch dạy hôm nay, thống kê nhanh, biểu đồ hoàn thành bài tập
        /// </summary>
        [HttpGet("Dashboard")]
        public IActionResult GetDashboard()
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            DateTime today = DateTime.Today;

            // ==================================================
            // 1. LỊCH DẠY HÔM NAY
            // ==================================================
            var todaySchedule = _context.BuoiHocs
                .Where(b => b.LopHoc.GiangVienId == giangVienId &&
                            b.ThoiGianBatDau.Date == today)
                .Select(b => new TeachingScheduleDto
                {
                    BuoiHocId = b.BuoiHocId.ToString(),
                    TenMon = b.LopHoc.MonHoc.TenMon,
                    TrangThai = b.TrangThai.ToString(),
                    PhongHoc = b.PhongHoc != null ? b.PhongHoc.TenPhong : "—",
                    GioBatDau = b.ThoiGianBatDau,
                    GioKetThuc = b.ThoiGianKetThuc,
                    SoLuongSinhVien = b.LopHoc.SinhVienLops.Count()
                })
                .OrderBy(x => x.GioBatDau)
                .ToList();

            // ==================================================
            // 2. THỐNG KÊ NHANH: TỔNG LỚP & TỔNG SINH VIÊN
            // ==================================================
            var totalClasses = _context.LopHocs
                .Count(l => l.GiangVienId == giangVienId);

            var totalStudents = _context.LopHocs
                .Where(l => l.GiangVienId == giangVienId)
                .SelectMany(l => l.SinhVienLops)
                .Select(s => s.SinhVienId)
                .Distinct()
                .Count();

            // ==================================================
            // 3. THÔNG BÁO MỚI (5 thông báo gần nhất)
            //    Bảng: ThongBao + ThongBaoNguoiDung + NguoiDung
            // ==================================================
            var notifications = _context.ThongBaoNguoiDungs
                .Where(t => t.NguoiDungId == giangVienId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new
                {
                    TieuDe = t.ThongBao.TieuDe,
                    ThoiGian = t.CreatedAt,
                    NguoiGui = t.ThongBao.NguoiDang.HoTen
                })
                .ToList();

            // ==================================================
            // 4. BIỂU ĐỒ HOÀN THÀNH BÀI TẬP
            //    Lấy trạng thái từng bài nộp
            // ==================================================
            var homeworkChart = _context.BaiNops
                .Where(bn => bn.BaiKiemTra.GiangVienId == giangVienId)
                .Select(bn => new HomeworkChartDto
                {
                    TenMon = bn.BaiKiemTra.MonHoc.TenMon,
                    TrangThai = bn.TrangThai
                })
                .ToList();

            // ==================================================
            // 5. HOẠT ĐỘNG GẦN ĐÂY — dùng NhatKyHeThong
            // ==================================================
            var recentActivities = _context.NhatKyHeThongs
            .Where(n => n.NguoiDungId == giangVienId)
    .OrderByDescending(n => n.CreatedAt)
    .Take(10)
    .Select(n => new RecentActivityDto
    {
        NhatKyHeThongId = n.NhatKyHeThongId,
        NoiDung = n.HanhDong ?? n.ThamChieu ?? "(Không có nội dung)",
        CreatedAt = n.CreatedAt
    })
    .ToList();


            // ==================================================
            // Gộp DTO Dashboard
            // ==================================================
            var dashboard = new InstructorDashboardDto
            {
                TodaySchedule = todaySchedule,
                TotalActiveClasses = totalClasses,
                TotalManagedStudents = totalStudents,
                HomeworkChart = homeworkChart
            };

            return Ok(new
            {
                Dashboard = dashboard,
                Notifications = notifications,
                RecentActivities = recentActivities
            });
        }


    }
}
