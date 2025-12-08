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
    public class GV_HoSoGiangVienController : Controller
    {
        private readonly AppDbContext _context;

        public GV_HoSoGiangVienController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giang-vien/ho-so")]
        public async Task<ActionResult<HoSoGiangVienDto>> GetHoSoGiangVien()
        {
            // Lấy GiangVien_id từ token
            var gvClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(gvClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Truy vấn giảng viên
            var giangVien = await _context.GiangViens
                .Where(gv => gv.GiangVienId == giangVienId)
                .Select(gv => new HoSoGiangVienDto
                {
                    GiangVienId = gv.GiangVienId,
                    MaSo = gv.Ms,

                    HoTen = gv.NguoiDung.HoTen,
                    Thumbnail = gv.NguoiDung.Avatar,
                    GioiTinh = gv.NguoiDung.GioiTinh,
                    NgaySinh = gv.NguoiDung.NgaySinh,

                    Email = gv.NguoiDung.Email,
                    SoDienThoai = gv.NguoiDung.SoDienThoai,

                    HocVi = gv.HocVi,
                    ChucVu = gv.ChucVu,

                    // Lấy tên khoa theo KhoaId
                    TenKhoa = gv.KhoaId != null
                        ? _context.Khoas.Where(k => k.KhoaId == gv.KhoaId)
                                       .Select(k => k.TenKhoa)
                                       .FirstOrDefault()
                        : null,

                    ChuyenMon = gv.ChuyenMon,

                    DiaChi = gv.NguoiDung.DiaChi,

                    // Tổng lớp giảng dạy
                    TongLopGiangDay = gv.GiangVienLops.Count(),

                    // Convert DateTime? → DateTime (fallback)
                    NgayTaoTaiKhoan = gv.NguoiDung.CreatedAt ?? DateTime.MinValue
                })
                .FirstOrDefaultAsync();

            if (giangVien == null)
                return NotFound("Không tìm thấy hồ sơ giảng viên");

            return Ok(giangVien);
        }


    }
}
