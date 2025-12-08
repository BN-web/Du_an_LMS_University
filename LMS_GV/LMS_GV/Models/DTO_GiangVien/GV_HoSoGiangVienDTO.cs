namespace LMS_GV.Models.DTO_GiangVien
{
    public class HoSoGiangVienDto
    {
        public int GiangVienId { get; set; }
        public string MaSo { get; set; }          // MS
        public string HoTen { get; set; }
        public string Thumbnail { get; set; }
        public string GioiTinh { get; set; }
        public DateOnly? NgaySinh { get; set; }

        public string Email { get; set; }
        public string SoDienThoai { get; set; }

        public string HocVi { get; set; }         // Tiến sĩ, Thạc sĩ...
        public string ChucVu { get; set; }        // Trưởng khoa, Giảng viên...

        public string TenKhoa { get; set; }
        public string ChuyenMon { get; set; }

        public string DiaChi { get; set; }

        public int TongLopGiangDay { get; set; }  // Tự tính
        public DateTime NgayTaoTaiKhoan { get; set; }
    }

}
