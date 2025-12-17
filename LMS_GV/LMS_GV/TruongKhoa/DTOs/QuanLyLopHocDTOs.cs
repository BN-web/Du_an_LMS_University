using System;
using System.Collections.Generic;

namespace LMS_GV.DTOs.TruongKhoa
{
    // DTO cho danh sách lớp học
    public class LopHocListDTO
    {
        public int LopHocId { get; set; }
        public string? MaLop { get; set; }
        public string? TenLop { get; set; }
        public string? TenMonHoc { get; set; }
        public string? TenGiangVien { get; set; }
        public string? TenKhoa { get; set; }
        public string? TenNganh { get; set; }
        public int? SiSoHienTai { get; set; }
        public int? SiSoToiDa { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
    }

    // DTO cho chi tiết lớp học
    public class LopHocDetailDTO
    {
        public int LopHocId { get; set; }
        public string? MaLop { get; set; }
        public string? TenLop { get; set; }
        public ThongTinMonHocDTO MonHoc { get; set; } = new ThongTinMonHocDTO();
        public ThongTinHocKyDTO HocKy { get; set; } = new ThongTinHocKyDTO();
        public ThongTinGiangVienDTO? GiangVien { get; set; }
        public int? SiSoHienTai { get; set; }
        public int? SiSoToiDa { get; set; }
        public int? SoTinChi { get; set; }
        public int? SoTiet { get; set; }
        public int? SoBuoiVangChoPhep { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? TrangThai { get; set; }
        public List<ThongTinSinhVienDTO> DanhSachSinhVien { get; set; } = new List<ThongTinSinhVienDTO>();
        public List<LichDayDTO> LichDay { get; set; } = new List<LichDayDTO>();
    }

    // DTO cho thông tin môn học
    public class ThongTinMonHocDTO
    {
        public int MonHocId { get; set; }
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }
        public int? SoTinChi { get; set; }
    }

    // DTO cho thông tin học kỳ
    public class ThongTinHocKyDTO
    {
        public int HocKyId { get; set; }
        public string? NamHoc { get; set; }
        public string? KiHoc { get; set; }
    }

    // DTO cho thông tin giảng viên
    public class ThongTinGiangVienDTO
    {
        public int GiangVienId { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? HocVi { get; set; }
        public string? ChuyenMon { get; set; }
    }

    // DTO cho thông tin sinh viên
    public class ThongTinSinhVienDTO
    {
        public int SinhVienId { get; set; }
        public string? MSSV { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? TenNganh { get; set; }
        public string? TenKhoaTuyenSinh { get; set; }
        public decimal? GPA { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayVaoLop { get; set; }
    }

    // DTO cho lịch dạy
    public class LichDayDTO
    {
        public int BuoiHocId { get; set; }
        public string? Thu { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public string? TenPhong { get; set; }
        public string? MaPhong { get; set; }
        public string? DiaChi { get; set; }
        public string? LoaiBuoiHoc { get; set; }
        public int SoBuoi { get; set; }
        public string? GhiChu { get; set; }
    }

    // DTO tạo lớp học mới
    public class TaoLopHocRequestDTO
    {
        public string? MaLop { get; set; }
        public string? TenLop { get; set; }
        public int MonHocId { get; set; }
        public int HocKyId { get; set; }
        public int? KhoaTuyenSinhId { get; set; }
        public int? KhoaId { get; set; }
        public int? NganhId { get; set; }
        public int? SiSoToiDa { get; set; }
        public int? SoTinChi { get; set; }
        public int? SoTiet { get; set; }
        public int? SoBuoiVangChoPhep { get; set; }
        public int? GiangVienId { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public byte? TrangThai { get; set; } = 1;
    }

    // DTO cập nhật lớp học
    public class CapNhatLopHocRequestDTO
    {
        public string? MaLop { get; set; }
        public string? TenLop { get; set; }
        public int? SiSoToiDa { get; set; }
        public int? SoTinChi { get; set; }
        public int? SoTiet { get; set; }
        public int? SoBuoiVangChoPhep { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public byte? TrangThai { get; set; }
    }

    // DTO phân công giảng viên
    public class PhanCongGiangVienRequestDTO
    {
        public int GiangVienId { get; set; }
        public DateTime? NgayBatDauPhanCong { get; set; }
        public string? GhiChu { get; set; }
    }

    // DTO thêm sinh viên vào lớp
    public class ThemSinhVienVaoLopRequestDTO
    {
        public int SinhVienId { get; set; }
        public string? TinhTrang { get; set; }
    }

    // DTO kiểm tra trùng lịch
    public class KiemTraTrungLichRequestDTO
    {
        public int GiangVienId { get; set; }
    }

    // DTO kết quả kiểm tra trùng lịch
    public class KiemTraTrungLichResponseDTO
    {
        public bool CoTrungLich { get; set; }
        public List<ChiTietTrungLichDTO> ChiTietTrungLich { get; set; } = new List<ChiTietTrungLichDTO>();
    }

    // DTO chi tiết trùng lịch
    public class ChiTietTrungLichDTO
    {
        public string? Thu { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public string? Phong { get; set; }
        public string? CoSo { get; set; }
        public string? LopHocTrung { get; set; }
    }

    // DTO tạo lịch dạy
    public class TaoLichDayRequestDTO
    {
        public string? Thu { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public int? PhongHocId { get; set; }
        public string? LoaiBuoiHoc { get; set; }
        public int SoBuoi { get; set; } = 1;
        public string? GhiChu { get; set; }
    }

    // DTO cập nhật lịch dạy
    public class CapNhatLichDayRequestDTO
    {
        public string? Thu { get; set; }
        public TimeSpan? GioBatDau { get; set; }
        public TimeSpan? GioKetThuc { get; set; }
        public int? PhongHocId { get; set; }
        public string? LoaiBuoiHoc { get; set; }
        public int? SoBuoi { get; set; }
        public string? GhiChu { get; set; }
    }

    // DTO tìm kiếm lớp học
    public class TimKiemLopHocDTO
    {
        public string? Search { get; set; }
        public int? MonHocId { get; set; }
        public int? KhoaId { get; set; }
        public int? NganhId { get; set; }
        public int? GiangVienId { get; set; }
        public byte? TrangThai { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    // DTO phản hồi thông thường cho Trưởng Khoa (đổi tên để tránh trùng với các namespace khác)
    public class TruongKhoaApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    // DTO phản hồi phân trang cho Trưởng Khoa (đổi tên để tránh trùng với các namespace khác)
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