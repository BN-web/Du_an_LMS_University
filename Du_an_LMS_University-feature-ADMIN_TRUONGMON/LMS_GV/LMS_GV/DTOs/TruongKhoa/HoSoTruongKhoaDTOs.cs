namespace LMS_GV.DTOs.TruongKhoa
{
    // DTO cho hồ sơ trưởng khoa
    public class HoSoTruongKhoaDTO
    {
        public string? MaGiangVien { get; set; }
        public string? HoTen { get; set; }
        public string? GioiTinh { get; set; }
        public string? NgaySinh { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? HocVi { get; set; }
        public string? ChucVu { get; set; }
        public string? TenKhoa { get; set; }
        public string? ChuyenMon { get; set; }
        public string? DiaChi { get; set; }
        public int TongSoLopGiangDay { get; set; }
        public string? NgayGiaNhap { get; set; }
        public string? TrangThai { get; set; }
        public string? Avatar { get; set; }
    }

    // DTO phản hồi thông thường cho Trưởng Khoa
    public class TruongKhoaApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    // DTO phản hồi phân trang cho Trưởng Khoa
    public class TruongKhoaPagedResponse<T>
    {
        public bool Success { get; set; } = true;
        public TruongKhoaPagedData<T> Data { get; set; } = new TruongKhoaPagedData<T>();
    }

    public class TruongKhoaPagedData<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
    }
}
