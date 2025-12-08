namespace LMS_GV.Models.DTO_GiangVien
{
    /// <summary>
    /// DTO: Buổi học trong lớp
    /// </summary>
    public class BuoiHocDTO
    {
        public int BuoiHoc_id { get; set; }
        public int LopHoc_id { get; set; }
        public string MaLop { get; set; }      // đổi từ TenLop → MaLop
        public string SoBuoi { get; set; }     // ví dụ: "Buổi 1", "Buổi 2"

        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }

        public string LoaiBuoiHoc { get; set; }   // giảng bài / lab / tutorial
        public byte? TrangThai { get; set; }      // 0=Đã hủy, 1=Bình thường, 2=Đổi giờ, 3=Đổi phòng, 4=Online 

        public string PhongHoc { get; set; }      // Null nếu online
        public string DiaChi { get; set; }

        public string Thu { get; set; }   // Tính từ ThoiGianBatDau
    }

    public class DanhSachBuoiHocResponse
    {
        public int LopHoc_id { get; set; }
        public string MaLop { get; set; }          // đổi từ TenLop → MaLop
        public string TenMonHoc { get; set; }
        public string TenGiangVien { get; set; }

        public List<BuoiHocDTO> BuoiHoc { get; set; }
    }

    //--- DTO List thông tin của 1 buổi điểm danh ----//
    /// <summary>
    /// Thông tin buổi học (cho nút Xem điểm danh)
    /// </summary>
    public class BuoiHocDanhSachDTO
    {
        public int BuoiHoc_id { get; set; }
        public string TenMonHoc { get; set; }
        public string MaLop { get; set; }        // Max 10 kí tự
        public string SoBuoi { get; set; }       // Buổi 1, Buổi 2...
        public DateTime ThoiGianBatDau { get; set; }
    }

    /// <summary>
    /// Thống kê điểm danh của 1 buổi
    /// </summary>
    public class ThongKeDiemDanhDTO
    {
        public int TongSinhVien { get; set; }
        public int TongCoMat { get; set; }
        public int TongVang { get; set; }
        public decimal TiLeCoMat { get; set; }   // %
    }

    /// <summary>
    /// Sinh viên trong lớp + trạng thái điểm danh
    /// </summary>
    public class SinhVienDiemDanhDTO
    {
        public int SinhVien_id { get; set; }
        public string MSSV { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string TrangThai { get; set; }    // co mat / vang
    }

    /// <summary>
    /// Dùng cho trả danh sách sinh viên kèm phân trang/search
    /// </summary>
    public class DanhSachSinhVienResponseFromLesson
    {
        public int BuoiHoc_id { get; set; }
        public List<SinhVienDiemDanhDTO> SinhVien { get; set; }
    }

    /// <summary>
    /// Dùng cho cập nhật trạng thái điểm danh
    /// </summary>
    public class CapNhatTrangThaiDiemDanhRequest
    {
        public int DiemDanhChiTiet_id { get; set; }
        public string TrangThai { get; set; }    // co mat / vang
    }
}
