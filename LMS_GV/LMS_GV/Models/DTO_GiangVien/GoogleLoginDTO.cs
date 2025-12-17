using System;

namespace LMS_GV.Models.DTO_GiangVien
{
    /// <summary>
    /// DTO request đăng nhập bằng Google
    /// Chỉ dùng cho Giảng Viên
    /// </summary>
    public class GoogleLoginRequestDTO
    {
        public string GoogleId { get; set; } = string.Empty; // ID từ Google
        public string Email { get; set; } = string.Empty;    // Email Google
        public string? Name { get; set; }                     // Tên hiển thị
        public string? PictureUrl { get; set; }              // Avatar
        public bool? EmailVerified { get; set; }             // Xác minh email
    }

    /// <summary>
    /// DTO thông tin người dùng trả về sau khi login
    /// Chỉ dùng cho Giảng Viên
    /// </summary>
    public class GoogleUserInfoDTO
    {
        public int NguoiDungId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string? Avatar { get; set; }
        public int? VaiTroId { get; set; }                 // Luôn = 2 (Giảng Viên)
        public string? TenVaiTro { get; set; }            // "Giảng Viên"
        public int? GiangVienId { get; set; }             // Id hồ sơ Giảng Viên
    }

    /// <summary>
    /// DTO response trả về sau khi login bằng Google
    /// </summary>
    public class GoogleLoginResponseDTO
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public string Token { get; set; } = string.Empty;  // JWT token
        public GoogleUserInfoDTO? User { get; set; }
    }
}
