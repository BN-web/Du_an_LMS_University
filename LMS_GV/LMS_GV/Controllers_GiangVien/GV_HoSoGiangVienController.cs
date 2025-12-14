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

        [Authorize(Roles = "Giảng Viên")]
        [HttpPut("giang-vien/avatar")]
        public async Task<IActionResult> UpdateAvatarGiangVien([FromForm] UploadAvatarGiangVienDto dto)
        {
            // 1. Lấy GiangVien_id từ token
            var gvClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(gvClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // 2. Truy giảng viên + người dùng
            var giangVien = await _context.GiangViens
                .Include(gv => gv.NguoiDung)
                .FirstOrDefaultAsync(gv => gv.GiangVienId == giangVienId);

            if (giangVien == null)
                return NotFound("Không tìm thấy giảng viên");

            // 3. Validate file
            if (dto.Avatar == null || dto.Avatar.Length == 0)
                return BadRequest("File ảnh không hợp lệ");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(dto.Avatar.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Chỉ cho phép ảnh jpg, jpeg, png, webp");

            if (dto.Avatar.Length > 5 * 1024 * 1024)
                return BadRequest("Dung lượng ảnh tối đa 5MB");

            // 4. Tạo thư mục lưu avatar
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // 5. Tạo tên file duy nhất
            var fileName = $"gv_{giangVienId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            // 6. Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Avatar.CopyToAsync(stream);
            }

            // 7. (OPTIONAL) Xóa avatar cũ
            if (!string.IsNullOrEmpty(giangVien.NguoiDung.Avatar))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", giangVien.NguoiDung.Avatar.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // 8. Update DB
            giangVien.NguoiDung.Avatar = $"/uploads/avatars/{fileName}";
            giangVien.NguoiDung.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Cập nhật avatar thành công",
                AvatarUrl = giangVien.NguoiDung.Avatar
            });
        }

    }
}
