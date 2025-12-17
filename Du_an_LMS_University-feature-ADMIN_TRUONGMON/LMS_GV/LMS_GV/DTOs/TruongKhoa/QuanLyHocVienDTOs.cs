using System;
using System.Collections.Generic;

namespace LMS_GV.DTOs.TruongKhoa
{
    // ============ DTOs CHO THỐNG KÊ ============
    
    public class ThongKeHocVienResponseDTO
    {
        public int TongSoHocVien { get; set; }
        public int HocVienHoatDong { get; set; }
        public int HocVienDaNghi { get; set; }
        public int TongSoLop { get; set; }
        public int TongGiangVien { get; set; }
        public List<ThongBaoGanDayDTO> ThongBaoGanDay { get; set; } = new List<ThongBaoGanDayDTO>();
        public List<BieuDoHoanThanhBaiTapDTO> BieuDoHoanThanhBaiTap { get; set; } = new List<BieuDoHoanThanhBaiTapDTO>();
    }

    public class ThongBaoGanDayDTO
    {
        public string NoiDung { get; set; } = string.Empty;
        public string ThoiGian { get; set; } = string.Empty;
    }

    public class BieuDoHoanThanhBaiTapDTO
    {
        public string MonHoc { get; set; } = string.Empty;
        public int DaNop { get; set; }
        public int ChuaNop { get; set; }
    }

    // ============ DTOs CHO DANH SÁCH HỌC VIÊN ============
    
    public class HocVienListDTO
    {
        public int Id { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? Khoa { get; set; }
        public string? TenKhoa { get; set; }
        public string? Nganh { get; set; }
        public string? MonLop { get; set; }
        public string? GiangVien { get; set; }
        public int? TongTinChi { get; set; }
        public decimal? Gpa { get; set; }
        public string? TrangThai { get; set; }
        public string? DanhGia { get; set; }
    }

    // ============ DTOs CHO CHI TIẾT HỌC VIÊN ============
    
    public class HocVienDetailResponseDTO
    {
        public ThongTinCaNhanDTO ThongTinCaNhan { get; set; } = new ThongTinCaNhanDTO();
        public List<BangDiemMonHocDTO> BangDiemMonHoc { get; set; } = new List<BangDiemMonHocDTO>();
    }

    public class ThongTinCaNhanDTO
    {
        public int Id { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? GioiTinh { get; set; }
        public string? NgaySinh { get; set; }
        public string? DienThoai { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? ChuyenNganh { get; set; }
        public string? Khoa { get; set; }
        public int? TongTinChi { get; set; }
        public string? NgayTaoHoSo { get; set; }
        public string? TrangThai { get; set; }
        public decimal? GpaTichLuy { get; set; }
    }

    public class BangDiemMonHocDTO
    {
        public string? MonHoc { get; set; }
        public decimal? DiemBaiTap { get; set; }
        public decimal? DiemGiuaKy { get; set; }
        public decimal? DiemCuoiKy { get; set; }
        public decimal? DiemChuyenCan { get; set; }
        public decimal? DiemTrungBinhMon { get; set; }
        public string? DiemChu { get; set; }
        public decimal? GpaMon { get; set; }
    }

    // ============ DTOs CHO LỊCH SỬ ĐIỂM ============
    
    public class LichSuDiemDTO
    {
        public string? ThoiGian { get; set; }
        public string? NguoiThucHien { get; set; }
        public string? MonHoc { get; set; }
        public string? ThanhPhanDiem { get; set; }
        public decimal? DiemCu { get; set; }
        public decimal? DiemMoi { get; set; }
        public string? LyDo { get; set; }
    }

    // ============ DTOs CHO TÌM KIẾM & LỌC ============
    
    public class TimKiemHocVienDTO
    {
        public string? Search { get; set; }
        public int? KhoaId { get; set; }
        public int? NganhId { get; set; }
        public int? KhoaTuyenSinhId { get; set; }
        public byte? TrangThai { get; set; }
        public int? LopHocId { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    // ============ DTOs CHO XUẤT BÁO CÁO ============
    
    public class XuatBaoCaoRequestDTO
    {
        public string Format { get; set; } = "pdf"; // "pdf" hoặc "excel"
        public string LoaiBaoCao { get; set; } = "danh-sach"; // "danh-sach" hoặc "chi-tiet"
        public List<int>? Ids { get; set; } // Danh sách ID học viên
        public int? KhoaId { get; set; }
        public int? NganhId { get; set; }
        public int? KhoaTuyenSinhId { get; set; }
    }
}
