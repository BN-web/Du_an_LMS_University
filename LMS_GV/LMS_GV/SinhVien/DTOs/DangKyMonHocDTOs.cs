using System;
using System.Collections.Generic;

namespace LMS_GV.DTOs.SinhVien
{
    // DTO cho danh sách môn có thể đăng ký
    public class MonHocDangKyDTO
    {
        public int Id { get; set; }
        public string? MaMon { get; set; }
        public string TenMon { get; set; } = null!;
        public string? GiangVien { get; set; }
        public string? EmailGiangVien { get; set; }
        public string? HocKy { get; set; }
        public string? NamHoc { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public int? SoTinChi { get; set; }
        public string TrangThai { get; set; } = "Chưa đăng ký";
        public int? LopHocId { get; set; }
        public string? MaLop { get; set; }
    }

    // DTO cho chi tiết môn học
    public class ChiTietMonHocDTO
    {
        public string? MaMon { get; set; }
        public string TenMon { get; set; } = null!;
        public string? GiangVien { get; set; }
        public string? EmailGiangVien { get; set; }
        public string? ThoiGianHoc { get; set; }
        public string? DiaDiem { get; set; }
        public int? TinChi { get; set; }
        public int? TongBuoi { get; set; }
        public int? SoBuoiDuocVang { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string? HocKy { get; set; }
        public string? NamHoc { get; set; }
    }

    // DTO cho request đăng ký môn
    public class DangKyMonHocRequestDTO
    {
        public int HocKyId { get; set; }
        public string? NamHoc { get; set; }
    }

    // DTO cho response đăng ký môn
    public class DangKyMonHocResponseDTO
    {
        public string? MaMon { get; set; }
        public string TenMon { get; set; } = null!;
        public string? LopHoc { get; set; }
        public string? GiangVien { get; set; }
        public string? HocKy { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string TrangThai { get; set; } = "Đã đăng ký";
    }

    // DTO cho kiểm tra trùng lịch
    public class KiemTraTrungLichDTO
    {
        public bool CoTrungLich { get; set; }
        public List<ChiTietTrungLichDTO>? ChiTietTrungLich { get; set; }
    }

    public class ChiTietTrungLichDTO
    {
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }
        public string? Thu { get; set; }
        public TimeSpan? GioBatDau { get; set; }
        public TimeSpan? GioKetThuc { get; set; }
        public DateTime? NgayTrung { get; set; }
    }

    // DTO cho môn đã đăng ký
    public class MonDaDangKyDTO
    {
        public int Id { get; set; }
        public string? MaMon { get; set; }
        public string TenMon { get; set; } = null!;
        public string? GiangVien { get; set; }
        public string? HocKy { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string? LopHoc { get; set; }
        public string TrangThai { get; set; } = "Đã đăng ký";
    }

    // DTO cho response API
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
    }
}