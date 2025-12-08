namespace LMS_GV.Models.DTO_GiangVien
{
    public class ThongBaoDto
    {
        public int ThongBaoId { get; set; }
        public string TieuDe { get; set; } = null!;
        public string? NoiDung { get; set; }
        public string? TenNguoiGui { get; set; }      // Lấy từ NguoiDang
        public string? NgayNhan { get; set; }  // Format dd/MM/yyyy HH:mm ( ngày/tháng/năm giờ:phút )
        public bool DaDoc { get; set; }               // Trạng thái đã đọc
    }


}
