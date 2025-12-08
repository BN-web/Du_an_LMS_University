namespace LMS_GV.Models.DTOs_GiangVien
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string MatKhau { get; set; }
    }

    public class TestHashDTO
    {
        public string Hash { get; set; }
        public List<string> Passwords { get; set; }
    }
}
