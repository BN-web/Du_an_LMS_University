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
        public int? DiemId { get; set; }     // ✅ dùng PATCH
        public int LopHocId { get; set; }    // ✅ xác định lớp

        public string TenMon { get; set; }
        public float? DiemBaiTap { get; set; }
        public float? DiemGiuaKy { get; set; }
        public float? DiemCuoiKy { get; set; }
        public float? ChuyenCan { get; set; }
        public float TrungBinhMon { get; set; }
        public float GPA_Mon { get; set; }
        public string DiemChu { get; set; }
        public int? SoTinChi { get; set; }
    }



    ///----------///
    public class PatchStudentScoreDto
    {
        [Required]
        public int SinhVienId { get; set; }

        [Required]
        public int LopHocId { get; set; }

        public decimal? DiemGiuaKy { get; set; }      // 0–10
        public decimal? DiemCuoiKy { get; set; }      // 0–10

        // ⚠️ Chỉ cho phép 10 | 9 | 0
        public decimal? DiemChuyenCan { get; set; }
    }

    public class TaoBangDiemRequestDTO
    {
        public int SinhVienId { get; set; }
        public int LopHocId { get; set; }
        public int HocKyId { get; set; }

        // 🔥 HỆ SỐ MÔN
        public decimal HeSoMon { get; set; }  // 1 | 1.5 | 2

        public List<DiemThanhPhanNhapDTO> DiemThanhPhans { get; set; } = new();
    }

    public class DiemThanhPhanNhapDTO
    {
        public string TenThanhPhan { get; set; } = string.Empty;
        public decimal Diem { get; set; }
        public string? GhiChu { get; set; }
    }



}
