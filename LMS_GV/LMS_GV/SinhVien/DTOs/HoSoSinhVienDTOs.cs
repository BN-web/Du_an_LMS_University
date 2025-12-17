using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS_GV.SinhVien.DTOs
{
    public class ThongTinCaNhanDTO
    {
        public string? AnhDaiDien { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(255, ErrorMessage = "Họ tên không được vượt quá 255 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        public string MaSv { get; set; } = string.Empty;

        [RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "Ngày sinh phải có định dạng dd/MM/yyyy")]
        public string? NgaySinh { get; set; }

        [StringLength(10, ErrorMessage = "Giới tính không được vượt quá 10 ký tự")]
        public string? GioiTinh { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string? DiaChi { get; set; }

        [Range(0, 4, ErrorMessage = "Điểm GPA phải từ 0 đến 4")]
        public decimal? DiemGPA { get; set; }

        [Range(0, 200, ErrorMessage = "Tổng tín chỉ phải từ 0 đến 200")]
        public int? TongTinChi { get; set; }

        public string? Khoa { get; set; }
        public string? Nganh { get; set; }

        [StringLength(50, ErrorMessage = "Thời gian đào tạo không được vượt quá 50 ký tự")]
        public string? ThoiGianDaoTao { get; set; }
    }

    public class MonDangHocDTO
    {
        public string? MaMon { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public string? Nganh { get; set; }
        public int? TinChi { get; set; }
        public string? GiangVien { get; set; }
        public string? EmailGiangVien { get; set; }
    }

    public class MonDaHoanThanhDTO
    {
        public string? MaMon { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public string? Nganh { get; set; }
        public int? TinChi { get; set; }
        public string? ThoiGian { get; set; }
        public string? GiangVien { get; set; }
        public string? EmailGiangVien { get; set; }
    }

    public class HoSoSinhVienResponseDTO
    {
        public ThongTinCaNhanDTO ThongTinCaNhan { get; set; } = new ThongTinCaNhanDTO();
        public List<MonDangHocDTO> DanhSachMonDangHoc { get; set; } = new List<MonDangHocDTO>();
        public List<MonDaHoanThanhDTO> LichSuMonDaHoanThanh { get; set; } = new List<MonDaHoanThanhDTO>();
    }

    // DTO cho request cập nhật avatar (hỗ trợ cả URL và path từ uploads)
    public class CapNhatAvatarRequestDTO
    {
        /// <summary>
        /// URL avatar (có thể là URL external hoặc path từ uploads như "/uploads/avatars/filename.jpg")
        /// </summary>
        [Required(ErrorMessage = "URL avatar là bắt buộc")]
        public string AvatarUrl { get; set; } = string.Empty;
    }

    // DTO cho danh sách ảnh có sẵn trong uploads
    public class DanhSachAnhResponseDTO
    {
        public List<AnhItemDTO> DanhSachAnh { get; set; } = new List<AnhItemDTO>();
    }

    public class AnhItemDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
    }

    // DTO cho response cập nhật avatar
    public class CapNhatAvatarResponseDTO
    {
        public string AvatarUrl { get; set; } = string.Empty;
        public string ThongBao { get; set; } = string.Empty;
    }

    // Thêm lại class UploadAvatarRequestDTO để tránh lỗi
    public class UploadAvatarRequestDTO
    {
        public IFormFile? File { get; set; }
    }

    // DTO cho response upload file
    public class UploadAvatarResponseDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ThongBao { get; set; } = string.Empty;
    }

    // DTO cho request tải ảnh từ URL
    public class TaiAnhTuUrlRequestDTO
    {
        [Required(ErrorMessage = "URL ảnh là bắt buộc")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string ImageUrl { get; set; } = string.Empty;
    }

    // DTO cho response tải ảnh từ URL
    public class TaiAnhTuUrlResponseDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ThongBao { get; set; } = string.Empty;
    }

    public class ApiResponseDTO<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }
}