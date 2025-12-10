namespace LMS_GV.Models.DTO_GiangVien
{
    /// Trang 1
    /// <summary> 
    /// DTO: Thông tin lớp của giảng viên
    /// </summary>
    public class LopHocGiangVienDto
    {
        public int LopHocId { get; set; }
        public string MaLop { get; set; }
        public string TenMon { get; set; }
        public int? SiSoHienTai { get; set; }
        public int? SiSoToiDa { get; set; }
        public string? Thu { get; set; }
        public string? GioBatDau { get; set; }
        public string? GioKetThuc { get; set; }

        // CHỈ LẤY SỐ TRONG MÃ PHÒNG
        public string? MaPhong { get; set; }
    }



    // ----Trang 4 ----//

    // Tổng quan thông tin lớp
    public class LopHocTongQuanDTO
    {
        public int LopHocId { get; set; }
        public string MaLop { get; set; }
        public string TenLop { get; set; }
        public int TongSinhVien { get; set; }
    }

    public class SinhVienTrongLopDTO
    {
        public int SinhVienId { get; set; }
        public string MSSV { get; set; }
        public string HoTen { get; set; }
        public string? TenNganh { get; set; }
        public decimal? TongDiem { get; set; }  // Điểm trung bình môn
        public byte? TrangThai { get; set; }     // trạng thái của sinh viên:-- 0=bị khóa, 1=đang hoạt động, 2=đã tốt nghiệp, 3=Dừng hoạt động, 4= cảnh báo 
    }

    public class DanhSachSinhVienResponse
    {
        public int LopHocId { get; set; }
        public string MaLop { get; set; }
        public string TenLop { get; set; }
        public int TongSinhVien { get; set; }
        public List<SinhVienTrongLopDTO> SinhVien { get; set; }
    }



    //----Trang 5-----//

    //Thông tin tổng quan sinh viên trong lớp
    public class SinhVienLopThongTinDTO
    {
        public int SinhVienId { get; set; }
        public int LopHocId { get; set; }

        public int TongBuoi { get; set; }
        public int TongCoMat { get; set; }
        public int TongVang { get; set; }
    }

    //Lịch sử điểm danh của sinh viên trong lớp
    public class SinhVienDiemDanhItemDTO
    {
        public string Buoi { get; set; }       // "Buổi 1"
        public string Ngay { get; set; }       // "01/09/2024"
        public string TrangThai { get; set; }  // "Có mặt / Vắng / Đi muộn / Chưa điểm danh"
    }

    //Thông tin chi tiết sinh viên (Profile + điểm + môn)
    public class DiemThanhPhanDTO
    {
        public string TenThanhPhan { get; set; }   // "Giữa kỳ"
        public decimal? Diem { get; set; }         // 8.5
    }

    //Điểm 
    public class DiemMonDTO
    {
        public string Mon { get; set; }              // "Lập trình C#"
        public string LopHoc { get; set; }           // "CSHARP_01"

        public decimal? DiemTrungBinhMon { get; set; }
        public string DiemChu { get; set; }          // "A", "B+", ...
        public decimal? GPAMon { get; set; }         // 4.0, 3.5, ...

        public List<DiemThanhPhanDTO> ThanhPhan { get; set; }
    }

    // Thông tin sinh viên
    public class SinhVienChiTietDTO
    {
        // Thông tin cá nhân
        public int SinhVienId { get; set; }
        public string MSSV { get; set; }
        public string HoTen { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }

        // Thông tin học tập
        public string TenNganh { get; set; }
        public string TenKhoa { get; set; }

        // Điểm các môn
        public List<DiemMonDTO> DiemMon { get; set; }
    }

}
