using LMS_GV.Models;
using LMS_GV.Models.Data;
using LMS_GV.Models.DTO_GiangVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;

namespace LMS_GV.Controllers_GiangVien
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Giảng Viên")]
    public class GV_LopHocController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GV_LopHocController(AppDbContext context)
        {
            _context = context;
        }

        //---Trang 1----//
        /// <summary>
        /// API 1: Lấy danh sách toàn bộ lớp học của giảng viên
        /// Không bao gồm chức năng tìm kiếm
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("danh-sach-lop-hoc")]
        public async Task<ActionResult<List<LopHocGiangVienDto>>> GetLopHocCuaGiangVien()
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var query = _context.LopHocs
                .Where(l => l.GiangVienId == giangVienId)
                .Select(l => new LopHocGiangVienDto
                {
                    LopHocId = l.LopHocId,
                    MaLop = l.MaLop,
                    TenMon = l.MonHoc.TenMon,
                    SiSoToiDa = l.SiSoToiDa,

                    //SiSoHienTai = l.SinhVienLops.Count(),

                    Thu = l.BuoiHocs
    .OrderBy(b => b.ThoiGianBatDau)
    .Select(b => b.Thứ)
    .FirstOrDefault(),


                    GioBatDau = l.BuoiHocs
                        .OrderBy(b => b.ThoiGianBatDau)
                        .Select(b => b.ThoiGianBatDau.ToString("HH:mm"))
                        .FirstOrDefault(),

                    GioKetThuc = l.BuoiHocs
                        .OrderBy(b => b.ThoiGianKetThuc)
                        .Select(b => b.ThoiGianKetThuc.ToString("HH:mm"))
                        .FirstOrDefault(),

                    TenPhong = l.BuoiHocs
                        .Select(b => b.PhongHoc.TenPhong)
                        .FirstOrDefault()
                });

            var result = await query.ToListAsync();
            return Ok(result);
        }

        /// <summary>
        /// API 2: Tìm kiếm lớp học theo mã lớp hoặc tên môn
        /// Điều kiện: phải bắt đầu bằng từ khóa (StartsWith)
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("lop-hoc/search")]
        public async Task<ActionResult<List<LopHocGiangVienDto>>> SearchLopHoc([FromQuery] string keyword)
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new List<LopHocGiangVienDto>());

            keyword = keyword.Trim();

            var query = _context.LopHocs
                .Where(l => l.GiangVienId == giangVienId)
                .Where(l =>
                    l.MaLop.StartsWith(keyword) ||
                    l.MonHoc.TenMon.StartsWith(keyword)
                )
                .Select(l => new LopHocGiangVienDto
                {
                    LopHocId = l.LopHocId,
                    MaLop = l.MaLop,
                    TenMon = l.MonHoc.TenMon,
                    SiSoToiDa = l.SiSoToiDa,

                    //SiSoHienTai = l.SinhVienLops.Count(),

                    Thu = l.BuoiHocs
                        .OrderBy(b => b.ThoiGianBatDau)
                        .Select(b => b.ThoiGianBatDau.DayOfWeek.ToString())
                        .FirstOrDefault(),

                    GioBatDau = l.BuoiHocs
                        .OrderBy(b => b.ThoiGianBatDau)
                        .Select(b => b.ThoiGianBatDau.ToString("HH:mm"))
                        .FirstOrDefault(),

                    GioKetThuc = l.BuoiHocs
                        .OrderBy(b => b.ThoiGianKetThuc)
                        .Select(b => b.ThoiGianKetThuc.ToString("HH:mm"))
                        .FirstOrDefault(),

                    TenPhong = l.BuoiHocs
                        .Select(b => b.PhongHoc.TenPhong)
                        .FirstOrDefault()
                });

            var result = await query.ToListAsync();
            return Ok(result);
        }

        //----Trang 4: Click nút "Xem điểm danh" (trang 2) -> Hiện bảng thông tin buổi học đã lưu (Không thể chỉnh sửa)----//
        // --- 1. Lấy tổng quan các lớp của giảng viên ---
        [HttpGet("tong-quan-lop")]
        public async Task<IActionResult> GetTongQuanLopCuaGiangVien()
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var lopHocs = await _context.LopHocs
                .Where(l => l.GiangVienId == giangVienId)
                .Select(l => new LopHocTongQuanDTO
                {
                    LopHocId = l.LopHocId,
                    MaLop = l.MaLop!,
                    TenLop = l.TenLop!,
                    TongSinhVien = l.SinhVienLops.Count()
                })
                .ToListAsync();

            return Ok(lopHocs);
        }

        // --- 2. Lấy danh sách sinh viên trong lớp thuộc giảng viên(token) ---
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("{lopHocId}/sinh-vien")]
        public async Task<IActionResult> GetDanhSachSinhVienTheoLop(int lopHocId)
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var lop = await _context.LopHocs
    .Include(l => l.SinhVienLops)
        .ThenInclude(svl => svl.SinhVien)
            .ThenInclude(hs => hs.NguoiDung)

    .Include(l => l.SinhVienLops)
        .ThenInclude(svl => svl.SinhVien)
            .ThenInclude(hs => hs.Diems)

    .FirstOrDefaultAsync(l => l.LopHocId == lopHocId
                           && l.GiangVienId == giangVienId);


            if (lop == null)
                return NotFound("Lớp không tồn tại hoặc không thuộc quyền giảng viên.");

            var danhSach = lop.SinhVienLops.Select(sv => new SinhVienTrongLopDTO
            {
                SinhVienId = sv.SinhVienId,
                MSSV = sv.SinhVien.Mssv ?? "",
                HoTen = sv.SinhVien.NguoiDung.HoTen ?? "",

                TenNganh = _context.Nganhs
              .Where(n => n.NganhId == sv.SinhVien.NganhId)
              .Select(n => n.TenNganh)
              .FirstOrDefault(),

                TongDiem = sv.SinhVien.Diems
              .Where(d => d.LopHocId == lopHocId)
              .Select(d => d.DiemTrungBinhMon)
              .FirstOrDefault(),
            })
      .ToList();


            var response = new DanhSachSinhVienResponse
            {
                LopHocId = lop.LopHocId,
                MaLop = lop.MaLop ?? "",
                TenLop = lop.TenLop ?? "",
                TongSinhVien = danhSach.Count(),
                SinhVien = danhSach
            };

            return Ok(response);
        }


        // --- 3. Search sinh viên trong lớp theo MSSV hoặc Họ tên ---
        [HttpGet("{lopHocId}/sinh-vien/search")]
        public async Task<IActionResult> SearchSinhVien(int lopHocId, [FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new List<SinhVienTrongLopDTO>());

            keyword = keyword.Trim().ToLower();
            string keywordND = RemoveDiacritics(keyword);

            // Validate token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // --- B1: Query DB (Không RemoveDau / Không xử lý chuỗi) ---
            var rawData = await _context.SinhVienLops
                .Where(sv => sv.LopHocId == lopHocId &&
                             sv.LopHoc.GiangVienId == giangVienId)
                .Select(sv => new
                {
                    sv.SinhVienId,
                    Mssv = sv.SinhVien.Mssv,
                    HoTen = sv.SinhVien.NguoiDung.HoTen,
                    TenNganh = _context.Nganhs
                                    .Where(n => n.NganhId == sv.SinhVien.NganhId)
                                    .Select(n => n.TenNganh)
                                    .FirstOrDefault(),
                    Diem = sv.SinhVien.Diems
                                .Where(d => d.LopHocId == lopHocId)
                                .Select(d => d.DiemTrungBinhMon)
                                .FirstOrDefault(),
                    TrangThai = sv.SinhVien.NguoiDung.TrangThai
                })
                .ToListAsync(); // 🟢 Lúc này EF Core hoàn toàn xử lý được

            // --- B2: Xử lý search trong C# ---
            var result = rawData
                .Where(sv =>
                    sv.Mssv?.ToLower().StartsWith(keyword) == true ||
                    sv.HoTen?.ToLower().StartsWith(keyword) == true ||
                    RemoveDiacritics(sv.HoTen ?? "").StartsWith(keywordND) ||
                    RemoveDiacritics(sv.HoTen ?? "").Split(' ')
                        .Any(t => t.StartsWith(keywordND))
                )
                .Select(sv => new SinhVienTrongLopDTO
                {
                    SinhVienId = sv.SinhVienId,
                    MSSV = sv.Mssv,
                    HoTen = sv.HoTen,
                    TenNganh = sv.TenNganh,
                    TongDiem = sv.Diem,
                    TrangThai = sv.TrangThai
                })
                .ToList();

            return Ok(result);
        }



        //-----Trang 5------//

        /// <summary>
        /// Lấy thông tin tổng quan điểm danh của 1 sinh viên trong 1 lớp.
        /// Giảng viên chỉ xem được lớp mà mình dạy.
        /// </summary>
        [HttpGet("lop/{lopHocId}/sinh-vien/{sinhVienId}/tong-quan")]
        public IActionResult GetThongTinTongQuanSinhVien(int lopHocId, int sinhVienId)
        {
            // Lấy giảng viên từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Kiểm tra lớp có thuộc giảng viên không
            var lop = _context.LopHocs
                .FirstOrDefault(l => l.LopHocId == lopHocId && l.GiangVienId == giangVienId);

            if (lop == null)
                return Unauthorized("Bạn không có quyền xem lớp này.");

            // Tổng số buổi học
            int tongBuoi = _context.BuoiHocs
                .Count(b => b.LopHocId == lopHocId);

            // Lấy chi tiết điểm danh của sinh viên
            var diemDanhChiTiet = _context.DiemDanhChiTiets
                .Where(ct => ct.SinhVienId == sinhVienId &&
                             ct.DiemDanh.LopHocId == lopHocId)
                .ToList();

            var dto = new SinhVienLopThongTinDTO
            {
                SinhVienId = sinhVienId,
                LopHocId = lopHocId,
                TongBuoi = tongBuoi,
                TongCoMat = diemDanhChiTiet.Count(d => d.TrangThai == "Có mặt"),
                TongVang = diemDanhChiTiet.Count(d => d.TrangThai == "Vắng")
            };

            return Ok(dto);
        }




        /// <summary>
        /// Lấy lịch sử điểm danh của sinh viên trong 1 lớp.
        /// Chỉ giảng viên dạy lớp đó mới xem được.
        /// </summary>
        [HttpGet("lop/{lopHocId}/sinh-vien/{sinhVienId}/diem-danh")]
        public IActionResult GetLichSuDiemDanh(int lopHocId, int sinhVienId)
        {
            // Lấy giảng viên từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Kiểm tra lớp thuộc quyền giảng viên
            var lop = _context.LopHocs
                .FirstOrDefault(l => l.LopHocId == lopHocId && l.GiangVienId == giangVienId);

            if (lop == null)
                return Unauthorized("Bạn không có quyền xem lớp này.");

            // Lấy tất cả điểm danh của sinh viên kèm DiemDanh
            var diemDanhChiTiets = _context.DiemDanhChiTiets
                .Include(ct => ct.DiemDanh)
                .Where(ct => ct.SinhVienId == sinhVienId)
                .ToList();

            // Lấy danh sách buổi học và kết hợp với điểm danh
            var buoiHocs = _context.BuoiHocs
                .Where(b => b.LopHocId == lopHocId)
                .OrderBy(b => b.SoBuoi)
                .AsEnumerable()
                .Select(b => new SinhVienDiemDanhItemDTO
                {
                    Buoi = $"Buổi {b.SoBuoi}",
                    Ngay = b.ThoiGianBatDau.ToString("dd/MM/yyyy"),
                    TrangThai = diemDanhChiTiets
                                .FirstOrDefault(ct => ct.DiemDanh != null && ct.DiemDanh.BuoiHocId == b.BuoiHocId)
                                ?.TrangThai ?? "Chưa điểm danh"
                })
                .ToList();

            return Ok(buoiHocs);
        }




        /// <summary>
        /// Lấy thông tin chi tiết sinh viên, bao gồm hồ sơ + điểm tất cả các môn.
        /// Giảng viên chỉ xem được sinh viên của lớp mình.
        /// </summary>
        [HttpGet("sinh-vien/{sinhVienId}/chi-tiet")]
        public IActionResult GetThongTinChiTietSinhVien(int sinhVienId)
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Kiểm tra sinh viên thuộc lớp giảng viên
            bool coQuyen = _context.SinhVienLops
                .Any(sv => sv.SinhVienId == sinhVienId &&
                           sv.LopHoc.GiangVienId == giangVienId);

            if (!coQuyen)
                return Unauthorized("Bạn không có quyền xem sinh viên này.");

            // Lấy hồ sơ sinh viên
            var sv = _context.HoSoSinhViens
                .Include(s => s.NguoiDung)
                .FirstOrDefault(s => s.SinhVienId == sinhVienId);

            if (sv == null)
                return NotFound("Không tìm thấy sinh viên.");

            // Lấy ngành
            var nganh = _context.Nganhs.FirstOrDefault(n => n.NganhId == sv.NganhId);

            // Lấy khoa của ngành
            var khoa = nganh != null
                ? _context.Khoas.FirstOrDefault(k => k.KhoaId == nganh.KhoaId)
                : null;

            // Lấy điểm các môn từ bảng Diems
            var diemMon = _context.Diems
                .Where(d => d.SinhVienId == sinhVienId)
                .Select(d => new DiemMonDTO
                {
                    Mon = d.LopHoc.MonHoc.TenMon,
                    LopHoc = d.LopHoc.MaLop,
                    DiemTrungBinhMon = d.DiemTrungBinhMon,
                    DiemChu = d.DiemChu,
                    GPAMon = d.Gpamon,

                    // Thành phần điểm – lấy theo Lớp + Sinh viên
                    ThanhPhan = _context.DiemThanhPhans
                        .Where(tp => tp.SinhVienId == sinhVienId && tp.LopHocId == d.LopHocId)
                        .Select(tp => new DiemThanhPhanDTO
                        {
                            TenThanhPhan = tp.ThanhPhanDiem.Ten,
                            Diem = tp.Diem
                        }).ToList()
                })
                .ToList();

            // Gộp vào DTO trả ra API
            var dto = new SinhVienChiTietDTO
            {
                SinhVienId = sv.SinhVienId,
                MSSV = sv.Mssv,
                HoTen = sv.NguoiDung.HoTen,
                NgaySinh = sv.NguoiDung.NgaySinh,
                Email = sv.NguoiDung.Email,
                SoDienThoai = sv.NguoiDung.SoDienThoai,
                DiaChi = sv.NguoiDung.DiaChi,

                TenNganh = nganh?.TenNganh,
                TenKhoa = khoa?.TenKhoa,

                DiemMon = diemMon
            };

            return Ok(dto);
        }




        // Hàm bỏ dấu
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }

    }
}
