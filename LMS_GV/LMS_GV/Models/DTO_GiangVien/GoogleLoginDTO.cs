using System.ComponentModel.DataAnnotations;
namespace LMS_GV.Models.DTO_GiangVien
{
    // Yêu cầu đăng nhập = Gg

        public class GoogleLoginRequestDTO
        {
            [Required]
            public string GoogleId { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            public string? Name { get; set; }
            public string? PictureUrl { get; set; }
            public bool? EmailVerified { get; set; }

            public string? AccessToken { get; set; }
            public string? RefreshToken { get; set; }
        }

        public class GoogleUserResponseDTO
        {
            public int NguoiDungId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? Avatar { get; set; }
            public string Role { get; set; } = "giảng viên";
            public string Token { get; set; } = string.Empty;
        }
    }

