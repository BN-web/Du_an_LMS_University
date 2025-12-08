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

    [Authorize(Roles = "Giảng Viên")]
    [Route("api/[controller]")]
    [ApiController]
    public class GV_ThongBaoController : Controller
    {
        private readonly AppDbContext _context;

        public GV_ThongBaoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/thongbao
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("Get/danh-sach-toan-bo-thong-bao-cua-giang-vien")]
        public async Task<ActionResult<List<ThongBaoDto>>> GetThongBaoCuaGiangVien()
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Truy vấn thông báo dành cho giảng viên
            var thongbaos = await _context.ThongBaoNguoiDungs
                .Where(t => t.NguoiDungId == giangVienId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new ThongBaoDto
                {
                    ThongBaoId = t.ThongBaoId,
                    TieuDe = t.ThongBao.TieuDe,
                    NoiDung = t.ThongBao.NoiDung,
                    TenNguoiGui = t.ThongBao.NguoiDang != null ? t.ThongBao.NguoiDang.HoTen : "Hệ thống",
                    NgayNhan = t.CreatedAt.HasValue ? t.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : null,
                    DaDoc = t.DaDoc ?? false
                })
                .ToListAsync();

            return Ok(thongbaos);
        }


        //Thông báo chưa đọc
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("thongbao/danh-sach-thong-bao-chua-doc")]
        public async Task<ActionResult<List<ThongBaoDto>>> GetThongBaoChuaDoc()
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var thongbaos = await _context.ThongBaoNguoiDungs
                .Where(t => t.NguoiDungId == giangVienId && t.DaDoc == false)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new ThongBaoDto
                {
                    ThongBaoId = t.ThongBaoId,
                    TieuDe = t.ThongBao.TieuDe,
                    NoiDung = t.ThongBao.NoiDung,
                    TenNguoiGui = t.ThongBao.NguoiDang != null ? t.ThongBao.NguoiDang.HoTen : "Hệ thống",
                    NgayNhan = t.CreatedAt.HasValue ? t.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : null,
                    DaDoc = t.DaDoc ?? false
                })
                .ToListAsync();

            return Ok(thongbaos);
        }


        // Thông báo đã đọc
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("thongbao/danh-sach-thong-bao-da-doc")]
        public async Task<ActionResult<List<ThongBaoDto>>> GetThongBaoDaDoc()
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var thongbaos = await _context.ThongBaoNguoiDungs
                .Where(t => t.NguoiDungId == giangVienId && t.DaDoc == true)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new ThongBaoDto
                {
                    ThongBaoId = t.ThongBaoId,
                    TieuDe = t.ThongBao.TieuDe,
                    NoiDung = t.ThongBao.NoiDung,
                    TenNguoiGui = t.ThongBao.NguoiDang != null ? t.ThongBao.NguoiDang.HoTen : "Hệ thống",
                    NgayNhan = t.CreatedAt.HasValue ? t.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : null,
                    DaDoc = t.DaDoc ?? false
                })
                .ToListAsync();

            return Ok(thongbaos);
        }

        //SideBar: Tổng số thông báo chưa đọc
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("thongbao/tong-thong-bao-chua-doc")]
        public async Task<ActionResult<int>> GetTongThongBaoChuaDoc()
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            int count = await _context.ThongBaoNguoiDungs
                .Where(t => t.NguoiDungId == giangVienId && t.DaDoc == false)
                .CountAsync();

            return Ok(count);
        }

        //Chức năng “click vào thông báo → đánh dấu đã đọc”
        [Authorize(Roles = "Giảng Viên")]
        [HttpPatch("thongbao/danh-dau-da-doc/{thongBaoId}")]
        public async Task<IActionResult> MarkAsRead(int thongBaoId)
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var tbNguoiDung = await _context.ThongBaoNguoiDungs
                .FirstOrDefaultAsync(t => t.ThongBaoId == thongBaoId && t.NguoiDungId == giangVienId);

            if (tbNguoiDung == null)
                return NotFound("Không tìm thấy thông báo cho giảng viên");

            tbNguoiDung.DaDoc = true;
            await _context.SaveChangesAsync();

            return Ok("Đã đánh dấu thông báo là đã đọc");
        }
        //Notes:

            //Mỗi khi giảng viên click, frontend gọi API này.

            //Sau đó, sidebar có thể tự động giảm số lượng thông báo chưa đọc (tong-chua-doc).

            //Tránh dùng trạng thái tạm trên frontend vì không đồng bộ với DB.
    }
}

