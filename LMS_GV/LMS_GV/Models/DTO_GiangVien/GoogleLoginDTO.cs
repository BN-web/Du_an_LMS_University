namespace LMS_GV.Models.DTO_GiangVien
{
    // Yêu cầu đăng nhập = Gg
    public class GoogleLoginRequestDTO
    {
        public string IdToken { get; set; } = null!;  // Token Google gửi về
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
        public string? PictureUrl { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    // Trả kết quả 
    public class GoogleUserResponseDTO
    {
        public int NguoiDungId { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Avatar { get; set; }
        public string Role { get; set; } = null!;
        public string Token { get; set; } = null!;
    }

}
