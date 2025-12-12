using System.ComponentModel.DataAnnotations;

namespace LMS_GV.Models.DTO_GiangVien
{
    public class StudentFilterDto
    {
        public string? Search { get; set; }         // Từ khóa: tên, MSSV
        public int? KhoaId { get; set; }
        public int? NganhId { get; set; }
        public int? KhoaHocId { get; set; }
        public int? LopHocId { get; set; }
        public string? TrangThai { get; set; }      // Hoạt động / Dừng
    }

    public class StudentSummaryDto
    {
        public int TongSinhVien { get; set; }
        public int DangHoatDong { get; set; }
        public int DungHoatDong { get; set; }
        public List<StudentListItemDto> SinhViens { get; set; }
    }

    public class StudentListItemDto
    {
        public int SinhVienId { get; set; }
        public string MSSV { get; set; }
        public string HoTen { get; set; }
        public string Khoa { get; set; }
        public string Nganh { get; set; }
        public string Lop { get; set; }
        public string TrangThai { get; set; }
    }


    ///-----------///
    public class StudentDetailDto
    {
        public string MSSV { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string HinhAnh { get; set; }
        public DateTime NgaySinh { get; set; }
        public string Khoa { get; set; }
        public string Nganh { get; set; }
        public string TrangThai { get; set; }
        public float TongTC { get; set; }
        public float GPA { get; set; }
        public List<MonHocBangDiemDto> BangDiem { get; set; }
    }

    public class MonHocBangDiemDto
    {
        public string TenMon { get; set; }
        public float? DiemBaiTap { get; set; }
        public float? DiemGiuaKy { get; set; }
        public float? DiemCuoiKy { get; set; }
        public float? ChuyenCan { get; set; }
        public float TrungBinhMon { get; set; }
        public string DiemChu { get; set; }
        public float GPA_Mon { get; set; }
        public int? SoTinChi { get; set; }
        public int? diemId { get; set; }
        public int? lophocId { get; set; }
    }


    ///----------///
    public class PatchStudentScoreDto
    {
        public int SinhVienId { get; set; }           // ID sinh viên
        public int LopHocId { get; set; }             // ID lớp học
        [Range(0, 10, ErrorMessage = "Điểm bài tập phải từ 0 đến 10")]
        public decimal? DiemBaiTap { get; set; }      // Điểm bài tập
        [Range(0, 10, ErrorMessage = "Điểm giữa kỳ phải từ 0 đến 10")]
        public decimal? DiemGiuaKy { get; set; }      // Điểm giữa kỳ
        [Range(0, 10, ErrorMessage = "Điểm cuối kỳ phải từ 0 đến 10")]
        public decimal? DiemCuoiKy { get; set; }      // Điểm cuối kỳ
        [Range(0, 10, ErrorMessage = "Điểm chuyên cần phải từ 0 đến 10")]
        public decimal? DiemChuyenCan { get; set; }   // Điểm chuyên cần
    }

}
