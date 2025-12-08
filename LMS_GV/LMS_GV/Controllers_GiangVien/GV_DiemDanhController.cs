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


namespace LMS_GV.Controllers_GiangVien
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Giảng Viên")]
    public class GV_DiemDanhController : Controller
    {
        private readonly AppDbContext _context;

        public GV_DiemDanhController(AppDbContext context)
        {
            _context = context;
        }

        //---Trang 2----//

        // 1.API Get danh sách buổi học theo lớp của giảng viên
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giangvien/buoihoc")]
        public async Task<IActionResult> GetBuoiHocCuaGiangVien()
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var lopHocs = await _context.LopHocs
                .Where(l => l.GiangVienId == giangVienId)
                .Select(l => new
                {
                    l.LopHocId,
                    MaLop = l.MaLop,
                    TenMon = l.MonHoc.TenMon,
                    TenGiangVien = l.GiangVien.NguoiDung.HoTen
                })
                .ToListAsync();

            if (!lopHocs.Any())
                return NotFound("Giảng viên chưa có lớp nào!");

            var lopIds = lopHocs.Select(l => l.LopHocId).ToList();

            var buoiHocsRaw = await _context.BuoiHocs
                .Where(b => lopIds.Contains(b.LopHocId))
                .OrderBy(b => b.LopHocId)
                .ThenBy(b => b.ThoiGianBatDau)
                .ToListAsync();

            var buoiHocs = new List<BuoiHocDTO>();
            foreach (var lop in lopHocs)
            {
                var buoiTrongLop = buoiHocsRaw
     .Where(b => b.LopHocId == lop.LopHocId)
     .Select((b, i) => new BuoiHocDTO
     {
         BuoiHoc_id = b.BuoiHocId,
         LopHoc_id = b.LopHocId,
         MaLop = lop.MaLop,
         SoBuoi = $"Buổi {i + 1}",
         ThoiGianBatDau = b.ThoiGianBatDau,
         ThoiGianKetThuc = b.ThoiGianKetThuc,
         LoaiBuoiHoc = b.LoaiBuoiHoc,
         TrangThai = b.TrangThai,
         PhongHoc = b.PhongHoc != null ? b.PhongHoc.TenPhong : "Online",
         DiaChi = b.PhongHoc != null ? b.PhongHoc.DiaChi : "Online",
         Thu = b.Thứ // hoặc dùng DayOfWeek nếu muốn tự tính
     })
     .ToList();


                buoiHocs.AddRange(buoiTrongLop);
            }

            var response = lopHocs.Select(l => new DanhSachBuoiHocResponse
            {
                LopHoc_id = l.LopHocId,
                MaLop = l.MaLop,
                TenMonHoc = l.TenMon,
                TenGiangVien = l.TenGiangVien,
                BuoiHoc = buoiHocs.Where(b => b.LopHoc_id == l.LopHocId).ToList()
            }).ToList();

            return Ok(response);
        }

        //Trang 3: Click nút "Điểm danh" trong danh sách buổi học (trang 2) -> Hiện thông tin buổi học và điểm danh sinh viên
        //-> Click nút"Lưu điểm danh" -> Khóa không cho đổi trạng thái điểm danh 

        //API: Lấy thong tin chung của buổi học 
        [HttpGet("giangvien/buoihoc/{buoiHocId}/thong-ke")]
        public async Task<IActionResult> GetThongKeDiemDanh(int buoiHocId)
        {
            // Lấy id giảng viên từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var diemDanh = await _context.DiemDanhs
                .FirstOrDefaultAsync(d => d.BuoiHocId == buoiHocId);

            if (diemDanh == null) return NotFound("Chưa có dữ liệu điểm danh");

            var chiTiets = await _context.DiemDanhChiTiets
                .Where(c => c.DiemDanhId == diemDanh.DiemDanhId)
                .ToListAsync();

            int tong = chiTiets.Count;
            int coMat = chiTiets.Count(c => c.TrangThai == "co mat");
            int vang = chiTiets.Count(c => c.TrangThai == "vang");

            var result = new ThongKeDiemDanhDTO
            {
                TongSinhVien = tong,
                TongCoMat = coMat,
                TongVang = vang,
                TiLeCoMat = tong > 0 ? Math.Round(coMat * 100m / tong, 2) : 0
            };

            return Ok(result);
        }


        //API: Lấy danh sách sinh viên + trạng thái
        [HttpGet("giangvien/buoihoc/{buoiHocId}/sinhvien")]
        public async Task<IActionResult> GetDanhSachSinhVienDiemDanh(int buoiHocId)
        {
            // Lấy id giảng viên từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");


            // Kiểm tra buổi học có thuộc giảng viên không
            var buoiHoc = await _context.BuoiHocs
                .Include(b => b.LopHoc)
                .FirstOrDefaultAsync(b => b.BuoiHocId == buoiHocId);

            if (buoiHoc == null)
                return NotFound("Buổi học không tồn tại");

            if (buoiHoc.LopHoc.GiangVienId != giangVienId)
                return Forbid("Bạn không có quyền xem buổi học này");

            // Lấy dữ liệu điểm danh
            var diemDanh = await _context.DiemDanhs
                .FirstOrDefaultAsync(d => d.BuoiHocId == buoiHocId);

            if (diemDanh == null)
                return NotFound("Chưa có dữ liệu điểm danh");

            var list = await _context.DiemDanhChiTiets
                .Where(c => c.DiemDanhId == diemDanh.DiemDanhId)
                .Join(_context.HoSoSinhViens,
                      c => c.SinhVienId,
                      s => s.SinhVienId,
                      (c, s) => new { c, s })
                .Join(_context.NguoiDungs,
                      cs => cs.s.NguoiDungId,
                      nd => nd.NguoiDungId,
                      (cs, nd) => new SinhVienDiemDanhDTO
                      {
                          SinhVien_id = cs.s.SinhVienId,
                          MSSV = cs.s.Mssv,        // ✔ LẤY MSSV ĐÚNG
                          HoTen = nd.HoTen,
                          Email = nd.Email,
                          TrangThai = cs.c.TrangThai
                      })
                .OrderBy(s => s.HoTen)
                .ToListAsync();

            return Ok(new DanhSachSinhVienResponseFromLesson
            {
                BuoiHoc_id = buoiHocId,
                SinhVien = list
            });
        }



        //API: Tìm kiếm sinh viên theo keyword
        [HttpGet("giangvien/buoihoc/{buoiHocId}/sinhvien/search")]
        public async Task<IActionResult> SearchSinhVienDiemDanh(int buoiHocId, [FromQuery] string keyword)
        {
            // Lấy id giảng viên từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Nếu keyword trống thì trả về danh sách rỗng
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new List<SinhVienDiemDanhDTO>());

            keyword = keyword.Trim();

            // Lấy dữ liệu điểm danh của buổi
            var diemDanh = await _context.DiemDanhs
                .FirstOrDefaultAsync(d => d.BuoiHocId == buoiHocId);

            if (diemDanh == null)
                return NotFound("Chưa có dữ liệu điểm danh");

            // Truy vấn danh sách sinh viên kèm trạng thái
            var sinhVienList = await _context.DiemDanhChiTiets
                .Where(c => c.DiemDanhId == diemDanh.DiemDanhId)
                .Join(
                    _context.HoSoSinhViens,
                    c => c.SinhVienId,
                    s => s.SinhVienId,
                    (c, s) => new { c, s }
                )
                .Join(
                    _context.NguoiDungs,
                    cs => cs.s.NguoiDungId,
                    nd => nd.NguoiDungId,
                    (cs, nd) => new SinhVienDiemDanhDTO
                    {
                        SinhVien_id = cs.s.SinhVienId,
                        MSSV = nd.TenDangNhap,  // Bạn có thể đổi thành SinhVienId nếu muốn
                        HoTen = nd.HoTen,
                        Email = nd.Email,
                        TrangThai = cs.c.TrangThai
                    }
                )
                .Where(s => s.HoTen.Contains(keyword) || s.MSSV.Contains(keyword) || s.Email.Contains(keyword))
                .OrderBy(s => s.HoTen)
                .ToListAsync();  // <-- await lấy List thực tế

            // Trả kết quả
            return Ok(new DanhSachSinhVienResponseFromLesson
            {
                BuoiHoc_id = buoiHocId,
                SinhVien = sinhVienList
            });
        }

        
    }
}
