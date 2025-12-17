using System;

namespace LMS_GV.DTOs
{
    // DTO cho request đăng nhập Google
    public class GoogleLoginRequestDTO
    {
        public string GoogleId { get; set; } = string.Empty; // ID từ Google
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? PictureUrl { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    // DTO cho response đăng nhập Google
    public class GoogleLoginResponseDTO
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public string Token { get; set; } = string.Empty;
        public GoogleUserInfoDTO? User { get; set; }
    }

    // DTO cho thông tin người dùng
    public class GoogleUserInfoDTO
    {
        public int NguoiDungId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string? Avatar { get; set; }
        public int? VaiTroId { get; set; }
        public string? TenVaiTro { get; set; }
        public int? SinhVienId { get; set; }
        public int? GiangVienId { get; set; }
        public string? MSSV { get; set; }
    }
}



