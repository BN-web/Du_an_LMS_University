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
using Microsoft.Data.SqlClient;


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
        public async Task<IActionResult> GetBuoiHocCuaGiangVien_SQL()
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var sql = @"
SELECT
    lh.LopHoc_id,
    lh.MaLop,
    mh.TenMon,
    nd.HoTen AS TenGiangVien,

    bh.BuoiHoc_id,
    bh.SoBuoi,
    bh.Thứ AS Thu,
    bh.ThoiGianBatDau,
    bh.ThoiGianKetThuc,
    bh.LoaiBuoiHoc,
    bh.TrangThai,

    ISNULL(ph.TenPhong, 'Online') AS TenPhong,
    ISNULL(ph.DiaChi, 'Online') AS DiaChi

FROM BuoiHoc bh
INNER JOIN LopHoc lh ON lh.LopHoc_id = bh.LopHoc_id
INNER JOIN MonHoc mh ON mh.MonHoc_id = lh.MonHoc_id
INNER JOIN GiangVien gv ON gv.GiangVien_id = lh.GiangVien_id
INNER JOIN NguoiDung nd ON nd.NguoiDung_id = gv.GiangVien_id
LEFT JOIN PhongHoc ph ON ph.PhongHoc_id = bh.PhongHoc_id

WHERE lh.GiangVien_id = @GiangVienId
ORDER BY lh.LopHoc_id, bh.ThoiGianBatDau;
";

            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("@GiangVienId", giangVienId));

            using var reader = await command.ExecuteReaderAsync();

            var list = new List<object>();

            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    LopHoc_id = reader["LopHoc_id"],
                    MaLop = reader["MaLop"].ToString(),
                    TenMonHoc = reader["TenMon"].ToString(),
                    TenGiangVien = reader["TenGiangVien"].ToString(),

                    BuoiHoc_id = reader["BuoiHoc_id"],
                    SoBuoi = "Buổi " + reader["SoBuoi"].ToString(),
                    Thu = reader["Thu"].ToString(),

                    ThoiGianBatDau = Convert.ToDateTime(reader["ThoiGianBatDau"]).ToString("dd/MM/yyyy HH:mm"),
                    ThoiGianKetThuc = Convert.ToDateTime(reader["ThoiGianKetThuc"]).ToString("dd/MM/yyyy HH:mm"),

                    LoaiBuoiHoc = reader["LoaiBuoiHoc"].ToString(),
                    TrangThai = reader["TrangThai"],

                    PhongHoc = reader["TenPhong"].ToString(),
                    DiaChi = reader["DiaChi"].ToString()
                });
            }

            return Ok(list);
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
                return Unauthorized("Bạn không có quyền xem buổi học này");

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
        // API: Tìm kiếm sinh viên theo keyword (chỉ MSSV, HoTen, Email)
        [HttpGet("giangvien/buoihoc/{buoiHocId}/sinhvien/search")]
        public async Task<IActionResult> SearchSinhVienDiemDanh(int buoiHocId, [FromQuery] string keyword)
        {
            // Lấy id giảng viên từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Nếu không nhập từ khóa → trả về danh sách rỗng
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new DanhSachSinhVienResponseFromLesson
                {
                    BuoiHoc_id = buoiHocId,
                    SinhVien = new List<SinhVienDiemDanhDTO>()
                });

            keyword = keyword.Trim().ToLower(); // Chuẩn hóa

            // Kiểm tra buổi học có thuộc giảng viên hay không
            var buoiHoc = await _context.BuoiHocs
                .Include(b => b.LopHoc)
                .FirstOrDefaultAsync(b => b.BuoiHocId == buoiHocId);

            if (buoiHoc == null)
                return NotFound("Buổi học không tồn tại");

            if (buoiHoc.LopHoc.GiangVienId != giangVienId)
                return Unauthorized("Bạn không có quyền xem buổi học này");

            // Lấy dữ liệu điểm danh
            var diemDanh = await _context.DiemDanhs
                .FirstOrDefaultAsync(d => d.BuoiHocId == buoiHocId);

            if (diemDanh == null)
                return NotFound("Chưa có dữ liệu điểm danh");

            // Truy vấn sinh viên + trạng thái
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
                          MSSV = cs.s.Mssv,
                          HoTen = nd.HoTen,
                          Email = nd.Email,
                          TrangThai = cs.c.TrangThai
                      })
                .Where(s =>
                    s.MSSV.ToLower().Contains(keyword) ||
                    s.HoTen.ToLower().Contains(keyword) ||
                    s.Email.ToLower().Contains(keyword)
                )
                .OrderBy(s => s.HoTen)
                .ToListAsync();

            return Ok(new DanhSachSinhVienResponseFromLesson
            {
                BuoiHoc_id = buoiHocId,
                SinhVien = list
            });
        }

    }
}
