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
using System.Globalization;


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


        //lấy lịch dạy theo tuần của 1 tháng 
        /// <summary>
        //API 1 – Lấy danh sách tuần của một tháng(theo lịch thực tế)

        //Dùng để FE hiển thị lịch tháng dạng calendar.

        //Chia tháng thành các tuần(Thứ 2 → Chủ nhật).

        //Trả về:

        //Tuần số

        //Ngày bắt đầu

        //Ngày kết thúc

        //FE chọn tuần nào → gọi API 2 để lấy lịch dạy.
        /// </summary>
        [HttpGet("lich-thang/{year}/{month}")]
        public IActionResult GetDanhSachTuan(int year, int month)
        {
            List<TuanTrongThangDto> weeks = new();

            DateTime firstDay = new(year, month, 1);

            // Tìm thứ 2 đầu tiên của tuần chứa ngày 1
            int delta = DayOfWeek.Monday - firstDay.DayOfWeek;
            if (delta > 0) delta -= 7;

            DateTime startOfWeek = firstDay.AddDays(delta);

            int weekNumber = 1;

            while ((startOfWeek.Month == month || startOfWeek.AddDays(6).Month == month)
       && startOfWeek.Year <= year + 1)
            {
                var endOfWeek = startOfWeek.AddDays(6);

                weeks.Add(new TuanTrongThangDto
                {
                    SoTuan = weekNumber++,
                    StartDate = startOfWeek,
                    EndDate = endOfWeek
                });

                startOfWeek = startOfWeek.AddDays(7);
            }

            return Ok(weeks);
        }


        //lấy lịch dạy theo tuần
        /// <summary>
        //API 2 – Lấy lịch dạy theo tuần dựa trên API 1

        //FE gửi StartDate và EndDate lấy từ API 1.

        //API trả về danh sách buổi dạy đúng tuần đó.
        /// </summary>
        [HttpPost("giang-vien/lich-day-tuan")]
        public IActionResult GetLichDayTheoTuan([FromBody] TuanRequestDto req)
        {
            // Lấy GiangVien_id từ token
            string giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Chuyển DateOnly sang DateTime
            DateTime startDate = req.StartDate.ToDateTime(new TimeOnly(0, 0, 0));
            DateTime endDate = req.EndDate.ToDateTime(new TimeOnly(23, 59, 59));

            var lich = _context.BuoiHocs
                .Include(b => b.LopHoc).ThenInclude(l => l.MonHoc)
                .Include(b => b.LopHoc).ThenInclude(l => l.SinhVienLops)
                .Include(b => b.LopHoc).ThenInclude(l => l.GiangVien).ThenInclude(g => g.NguoiDung)
                .Include(b => b.PhongHoc)
                .Where(b => b.LopHoc.GiangVienId == giangVienId
                    && b.ThoiGianBatDau >= startDate
                    && b.ThoiGianBatDau <= endDate)
                .AsEnumerable()
                .Select(b => new LichDayTuanDto
                {
                    BuoiHocId = b.BuoiHocId,
                    MonHoc = b.LopHoc.MonHoc.TenMon,
                    Lop = b.LopHoc.MaLop,
                    GiangVien = b.LopHoc.GiangVien.NguoiDung.HoTen,
                    Ngay = b.ThoiGianBatDau.Date,
                    Thu = b.ThoiGianBatDau.ToString("dddd"),
                    GioBatDau = b.ThoiGianBatDau.ToString("HH:mm"),
                    GioKetThuc = b.ThoiGianKetThuc.ToString("HH:mm"),
                    Phong = b.PhongHoc?.TenPhong ?? "Chưa phân phòng",
                    DiaDiem = b.PhongHoc?.DiaChi ?? "—",
                    LoaiBuoiHoc = b.LoaiBuoiHoc ?? "Bình thường",
                    TrangThai = b.TrangThai switch
                    {
                        1 => "Bình thường",
                        0 => "Đã hủy",
                        2 => "Đổi giờ",
                        3 => "Đổi phòng",
                        4 => "Online",
                        _ => "Khác"
                    },
                    SoLuongSinhVien = b.LopHoc.SinhVienLops.Count()
                })
                .OrderBy(x => x.Ngay)
                .ThenBy(x => x.GioBatDau)
                .ToList();

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
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("danh-sach-cac-buoi-thi")]
        public IActionResult GetLichThi(DateTime? startDate, DateTime? endDate)
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Query cơ bản
            var query = _context.BuoiThis
                .Where(b => b.GiamThiId == giangVienId);

            // Lọc ngày thi
            if (startDate.HasValue)
                query = query.Where(b => b.NgayThi.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(b => b.NgayThi.Date <= endDate.Value.Date);

            // Include dữ liệu
            var data = query
                .Include(b => b.PhongHoc)
                .Include(b => b.LopHoc).ThenInclude(l => l.MonHoc)
                .Include(b => b.GiamThi).ThenInclude(g => g.NguoiDung)
                .Include(b => b.LopHoc.SinhVienLops)
                .ToList()
                .Select(b =>
                {
                    // ⭐ LẤY RA SỐ PHÒNG TỪ MA PHÒNG
                    // Ví dụ: "PH101" → "101"
                    string soPhong = b.PhongHoc?.MaPhong != null
                        ? new string(b.PhongHoc.MaPhong.Where(char.IsDigit).ToArray())
                        : "";

                    // ⭐ Địa điểm từ DB: DiaChi + số phòng
                    string diaDiem = b.PhongHoc != null
                        ? $"{b.PhongHoc.DiaChi} - {soPhong}"
                        : "";

                    return new LichThiDto
                    {
                        BuoiThiId = b.BuoiThiId,
                        MonHoc = b.LopHoc?.MonHoc?.TenMon ?? "",
                        Lop = b.LopHoc?.MaLop ?? "",
                        NgayThi = b.NgayThi.ToString("dddd/dd/MM/yyyy"),
                        GioBatDau = b.GioBatDau?.ToString(@"hh\:mm") ?? "",
                        GioKetThuc = b.GioKetThuc?.ToString(@"hh\:mm") ?? "",
                        GiangVien = b.GiamThi?.NguoiDung?.HoTen ?? "",
                        Loai = b.HinhThuc ?? "",
                        TongSinhVien = b.LopHoc?.SinhVienLops?.Count() ?? 0,
                        DiaDiem = diaDiem
                    };
                })
                .OrderBy(x => x.NgayThi)
                .ThenBy(x => x.GioBatDau)
                .ToList();

            return Ok(data);
        }

    }

}
