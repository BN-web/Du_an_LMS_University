using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using LMS_GV.DTOs.TruongKhoa;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LMS_GV.Controllers.TruongKhoa
{
    [Route("api/truong-khoa/ho-so")]
    [ApiController]
    [Authorize]
    public class TruongKhoaHoSoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TruongKhoaHoSoController> _logger;

        public TruongKhoaHoSoController(AppDbContext context, ILogger<TruongKhoaHoSoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy ID Trưởng khoa từ token
        private int GetTruongKhoaId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng");
        }

        // Helper: Chuyển trạng thái số sang chuỗi
        private string GetTrangThaiString(byte? trangThai)
        {
            return trangThai switch
            {
                1 => "Đang hoạt động",
                2 => "Tạm khóa",
                3 => "Đã nghỉ",
                _ => "Không xác định"
            };
        }

        // Helper: Chuyển giới tính sang tiếng Việt
        private string GetGioiTinhString(string? gioiTinh)
        {
            if (string.IsNullOrWhiteSpace(gioiTinh))
                return "Không xác định";

            return gioiTinh.ToLower() switch
            {
                "male" or "nam" or "m" => "Nam",
                "female" or "nữ" or "f" => "Nữ",
                _ => gioiTinh
            };
        }

        /// <summary>
        /// Lấy hồ sơ trưởng khoa
        /// GET: /api/truong-khoa/ho-so
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<TruongKhoaApiResponse<HoSoTruongKhoaDTO>>> GetHoSoTruongKhoa()
        {
            try
            {
                var truongKhoaId = GetTruongKhoaId();

                // Lấy thông tin người dùng
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(nd => nd.NguoiDungId == truongKhoaId);

                if (nguoiDung == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                // Kiểm tra VaiTroId trước (nhanh hơn)
                if (nguoiDung.VaiTroId != 3)
                {
                    _logger.LogWarning($"User {truongKhoaId} có VaiTroId = {nguoiDung.VaiTroId}, không phải Trưởng khoa (VaiTroId = 3)");
                    return Unauthorized(new TruongKhoaApiResponse<object>
                    {
                        Success = false,
                        Message = $"Bạn không phải là Trưởng khoa (VaiTroId: {nguoiDung.VaiTroId})"
                    });
                }

                // Lấy thông tin giảng viên (trưởng khoa)
                var giangVien = await _context.GiangViens
                    .Include(gv => gv.NguoiDung)
                    .Include(gv => gv.LopHocs)
                    .FirstOrDefaultAsync(gv => gv.NguoiDungId == truongKhoaId);

                // Nếu chưa có GiangVien, tạo mới với ChucVu = "Trưởng khoa"
                if (giangVien == null)
                {
                    giangVien = new GiangVien
                    {
                        NguoiDungId = truongKhoaId,
                        ChucVu = "Trưởng khoa",
                        KhoaId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.GiangViens.Add(giangVien);
                    await _context.SaveChangesAsync();
                    
                    // Reload để có Include
                    giangVien = await _context.GiangViens
                        .Include(gv => gv.NguoiDung)
                        .Include(gv => gv.LopHocs)
                        .FirstOrDefaultAsync(gv => gv.GiangVienId == giangVien.GiangVienId);
                }
                else
                {
                    // Đảm bảo ChucVu = "Trưởng khoa" nếu chưa có
                    if (giangVien.ChucVu != "Trưởng khoa")
                    {
                        giangVien.ChucVu = "Trưởng khoa";
                        giangVien.UpdatedAt = DateTime.Now;
                        _context.GiangViens.Update(giangVien);
                        await _context.SaveChangesAsync();
                    }
                }

                // Lấy thông tin khoa
                var khoa = await _context.Khoas
                    .FirstOrDefaultAsync(k => k.KhoaId == giangVien.KhoaId);

                // Đếm tổng số lớp giảng dạy (tất cả các lớp mà giảng viên này đã dạy)
                var tongSoLopGiangDay = await _context.LopHocs
                    .CountAsync(l => l.GiangVienId == giangVien.GiangVienId);

                // Format mã giảng viên (đảm bảo 5 chữ số)
                string maGiangVien = "00001";
                if (!string.IsNullOrWhiteSpace(giangVien.Ms))
                {
                    // Nếu Ms là số, format thành 5 chữ số
                    if (int.TryParse(giangVien.Ms, out int msNumber))
                    {
                        maGiangVien = msNumber.ToString("D5"); // Format thành 5 chữ số với leading zeros
                    }
                    else
                    {
                        maGiangVien = giangVien.Ms;
                    }
                }
                else
                {
                    // Nếu không có Ms, dùng NguoiDungId format thành 5 chữ số (để đảm bảo mã nhất quán với user)
                    maGiangVien = nguoiDung.NguoiDungId.ToString("D5");
                }

                // Tạo DTO response
                var hoSo = new HoSoTruongKhoaDTO
                {
                    MaGiangVien = maGiangVien,
                    HoTen = nguoiDung.HoTen,
                    GioiTinh = GetGioiTinhString(nguoiDung.GioiTinh),
                    NgaySinh = nguoiDung.NgaySinh.HasValue 
                        ? nguoiDung.NgaySinh.Value.ToString("dd/MM/yyyy") 
                        : null,
                    Email = nguoiDung.Email,
                    SoDienThoai = nguoiDung.SoDienThoai,
                    HocVi = giangVien.HocVi,
                    ChucVu = giangVien.ChucVu,
                    TenKhoa = khoa?.TenKhoa,
                    ChuyenMon = giangVien.ChuyenMon,
                    DiaChi = nguoiDung.DiaChi,
                    TongSoLopGiangDay = tongSoLopGiangDay,
                    NgayGiaNhap = giangVien.CreatedAt.HasValue 
                        ? giangVien.CreatedAt.Value.ToString("dd/MM/yyyy") 
                        : null,
                    TrangThai = GetTrangThaiString(nguoiDung.TrangThai),
                    Avatar = nguoiDung.Avatar
                };

                return Ok(new TruongKhoaApiResponse<HoSoTruongKhoaDTO>
                {
                    Success = true,
                    Message = "Lấy hồ sơ thành công",
                    Data = hoSo
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new TruongKhoaApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hồ sơ trưởng khoa");
                return StatusCode(500, new TruongKhoaApiResponse<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}
