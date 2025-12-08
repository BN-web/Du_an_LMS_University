using Humanizer;
using LMS_GV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Net.NetworkInformation;
using LMS_GV.Models.Data;
using LMS_GV.Models.DTO_GiangVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace LMS_GV.Controllers_GiangVien
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Giảng Viên")]
    public class GV_LichDayController : Controller
    {
        private readonly AppDbContext _context;

        public GV_LichDayController(AppDbContext context)
        {
            _context = context;
        }


        //lấy lịch dạy theo tuần
        /// <summary>
        /// Lấy lịch dạy theo tuần (calendar tuần)
        /// </summary>
        [HttpGet("Get/Lich-day-tuan-cua-giang-vien")]
        public IActionResult GetLichDayTheoTuan()
        {
            // Lấy GiangVien_id từ claim
            string giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
            {
                return Unauthorized("GiangVien_id không hợp lệ");
            }

            // Tính ngày bắt đầu và kết thúc tuần hiện tại
            DateTime today = DateTime.Today;
            int delta = (int)(1 - today.DayOfWeek);
            if (delta > 0) delta -= 7;
            DateTime startDate = today.AddDays(delta);
            DateTime endDate = startDate.AddDays(6).AddHours(23).AddMinutes(59);

            // Truy vấn dữ liệu
            var lich = _context.BuoiHocs
                .Include(b => b.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                .Include(b => b.LopHoc)
                    .ThenInclude(l => l.SinhVienLops)
                .Include(b => b.LopHoc)
                    .ThenInclude(l => l.GiangVien)
                        .ThenInclude(g => g.NguoiDung)
                .Include(b => b.PhongHoc)
                .Where(b => b.LopHoc.GiangVienId == giangVienId
                            && b.ThoiGianBatDau >= startDate
                            && b.ThoiGianBatDau <= endDate)
                .AsEnumerable() // chuyển sang LINQ to Objects để tránh lỗi EF
                .Select(b => new LichDayTuanDto
                {
                    BuoiHocId = b.BuoiHocId,
                    TenMon = b.LopHoc.MonHoc.TenMon,
                    Lop = b.LopHoc.MaLop,
                    GiangVien = b.LopHoc.GiangVien.NguoiDung.HoTen,
                    Ngay = b.ThoiGianBatDau.Date,
                    GioBatDau = b.ThoiGianBatDau.ToString("HH:mm"),
                    GioKetThuc = b.ThoiGianKetThuc.ToString("HH:mm"),
                    Phong = b.PhongHoc != null ? b.PhongHoc.TenPhong : "Online",
                    DiaDiem = b.PhongHoc != null ? b.PhongHoc.DiaChi : "Online",
                    LoaiBuoiHoc = b.LoaiBuoiHoc,
                    SoLuongSinhVien = b.LopHoc.SinhVienLops.Count(),
                    TrangThai = b.TrangThai switch
                    {
                        1 => "Bình thường",
                        0 => "Đã hủy",
                        2 => "Đổi giờ",
                        3 => "Đổi phòng",
                        4 => "Online",
                        _ => "Khác"
                    }
                })
                .OrderBy(x => x.Ngay)
                .ThenBy(x => x.GioBatDau)
                .ToList();

            // Trả dữ liệu
            return Ok(new
            {
                TuNgay = startDate.ToString("dd/MM/yyyy"),
                DenNgay = endDate.ToString("dd/MM/yyyy"),
                LichDay = lich
            });
        }



        /// <summary>
        /// Lấy chi tiết 1 buổi học theo BuoiHocId.
        /// Giảng viên chỉ xem được buổi học thuộc lớp mình dạy.
        /// </summary>
        [HttpGet("chi-tiet/{id}/Chi-tiet-1-buoi-hoc")]
        public IActionResult GetChiTietBuoiHoc(int id)
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var buoiHoc = _context.BuoiHocs
                .Where(b => b.BuoiHocId == id && b.LopHoc.GiangVienId == giangVienId)
                .Select(b => new BuoiHocChiTietDto
                {
                    BuoiHocId = b.BuoiHocId,

                    MonHoc = b.LopHoc.MonHoc.TenMon,
                    Lop = b.LopHoc.MaLop,
                    GiangVien = b.LopHoc.GiangVien.NguoiDung.HoTen
,

                    Ngay = b.ThoiGianBatDau.Date,
                    GioBatDau = b.ThoiGianBatDau.ToString("HH:mm"),
                    GioKetThuc = b.ThoiGianKetThuc.ToString("HH:mm"),

                    Phong = b.PhongHoc != null ? b.PhongHoc.TenPhong : "Chưa phân phòng",

                    DiaDiem = b.PhongHoc != null
                        ? b.PhongHoc.DiaChi
                        : "—",

                    LoaiBuoiHoc = b.LoaiBuoiHoc ?? "Bình thường",
                    SoBuoi = b.SoBuoi,
                    TrangThai = b.TrangThai == 1 ? "Bình thường" : "Khác"
                })
                .FirstOrDefault();

            if (buoiHoc == null)
                return NotFound("Buổi học không tồn tại hoặc không thuộc quyền của giảng viên.");

            return Ok(buoiHoc);
        }


        /// <summary>
        /// Lấy danh sách lịch sự kiện của giảng viên.
        /// Bao gồm: sự kiện lớp học, sự kiện buổi học.
        /// </summary>
        [HttpGet("su-kien")]
        public IActionResult GetLichSuKien()
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var suKiens = _context.LichSuKiens
                .Where(s => s.LopHoc!.GiangVienId == giangVienId ||
                            s.BuoiHoc!.LopHoc.GiangVienId == giangVienId)
                .Select(s => new LichSuKienDto
                {
                    LichSuKienId = s.LichSuKienId,

                    TieuDe = s.TieuDe,
                    Loai = s.Loai,
                    NgayBatDau = s.NgayBatDau,
                    NgayKetThuc = s.NgayKetThuc,

                    LopHoc = s.LopHoc != null ? s.LopHoc.MaLop : "",
                    BuoiHoc = s.BuoiHoc != null
                        ? $"{s.BuoiHoc.ThoiGianBatDau:dd/MM/yyyy HH:mm}"
                        : "",

                    MoTa = s.MoTa ?? ""
                })
                .OrderByDescending(s => s.NgayBatDau)
                .ToList();

            return Ok(suKiens);
        }


        //----------------------//

        //danh sách các buổi thi
        [HttpGet("danh-sach-cac-buoi-thi")]
        public IActionResult GetLichThi(DateTime? startDate, DateTime? endDate)
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var query = _context.BuoiThis
                .Where(b => b.GiamThiId == giangVienId);

            if (startDate.HasValue)
                query = query.Where(b => b.NgayThi.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(b => b.NgayThi.Date <= endDate.Value.Date);

            var lichThi = query
                .Select(b => new LichThiDto
                {
                    BuoiThiId = b.BuoiThiId,
                    MonHoc = b.LopHoc.MonHoc.TenMon,
                    Lop = b.LopHoc.MaLop,
                    NgayThi = b.NgayThi.ToString("dddd/dd/MM/yyyy"), // Thứ/ngày/tháng/năm
                    GioBatDau = b.GioBatDau.HasValue ? b.GioBatDau.Value.ToString(@"hh\:mm") : "",
                    GioKetThuc = b.GioKetThuc.HasValue ? b.GioKetThuc.Value.ToString(@"hh\:mm") : "",
                    GiangVien = b.GiamThi.NguoiDung.HoTen,
                    Loai = b.HinhThuc ?? "offline",
                    TongSinhVien = b.LopHoc.SinhVienLops.Count(), // đúng theo DB hiện tại
                    DiaDiem = b.PhongHoc != null
                              ? $"{b.PhongHoc.DiaChi} - {b.PhongHoc.TenPhong}"
                              : "Online"
                })
                .OrderBy(b => b.NgayThi)
                .ThenBy(b => b.GioBatDau)
                .ToList();

            return Ok(lichThi);
        }
    }

    }
