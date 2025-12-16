using System.ComponentModel.DataAnnotations;
namespace LMS_GV.Models.DTO_GiangVien
{
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
    }

    public class SendOtpResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    public class VerifyOtpRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Otp { get; set; } = string.Empty;
    }


}

