using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace LMS_GV.SinhVien.DTOs
{
    // DTO cho request lấy thời khóa biểu theo tuần (StartDate/EndDate)
    public class TuanRequestDto
    {
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }
    }

    // DTO cho tuần trong tháng
    public class TuanTrongThangDto
    {
        public int SoTuan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class ThoiKhoaBieuTuanResponseDTO
    {
        public string TuNgay { get; set; } = string.Empty;
        public string DenNgay { get; set; } = string.Empty;
        public List<ThoiKhoaBieuNgayDTO> ThoiKhoaBieu { get; set; } = new List<ThoiKhoaBieuNgayDTO>();
    }

    public class ThoiKhoaBieuNgayDTO
    {
        public string Thu { get; set; } = string.Empty;
        public string Ngay { get; set; } = string.Empty;
        public List<BuoiHocTKBItemDTO> BuoiHoc { get; set; } = new List<BuoiHocTKBItemDTO>();
        public int TongSoBuoi { get; set; }
    }

    public class BuoiHocTKBItemDTO
    {
        public int Id { get; set; }
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }
        public string? Phong { get; set; }
        public string? ThoiGian { get; set; }
        public string? GioBatDau { get; set; }
        public string? GioKetThuc { get; set; }
        public string? TrangThai { get; set; }
        public string? LoaiBuoiHoc { get; set; }
        public string? LopHoc { get; set; }
    }

    public class ChiTietBuoiHocDTO
    {
        public int Id { get; set; }
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }
        public string? Thu { get; set; }
        public string? GioBatDau { get; set; }
        public string? GioKetThuc { get; set; }
        public string? TrangThai { get; set; }
        public DiaDiemDTO? DiaDiem { get; set; }
        public GiangVienDTO? GiangVien { get; set; }
        public string? ChuKyTuan { get; set; }
        public string? ThoiGianBatDau { get; set; }
        public string? ThoiGianKetThuc { get; set; }
        public string? GhiChu { get; set; }
    }

    public class DiaDiemDTO
    {
        public string? CoSo { get; set; }
        public string? ToaNha { get; set; }
        public string? PhongHoc { get; set; }
    }

    public class GiangVienDTO
    {
        public string? Ten { get; set; }
        public string? Email { get; set; }
    }

    public class LichThiResponseDTO
    {
        public List<LichThiItemDTO> LichThi { get; set; } = new List<LichThiItemDTO>();
    }

    public class LichThiItemDTO
    {
        public int Id { get; set; }
        public string? MonThi { get; set; }
        public string? MaMon { get; set; }
        public string? Lop { get; set; }
        public string? NgayThi { get; set; }
        public string? GioBatDau { get; set; }
        public string? GioKetThuc { get; set; }
        public string? GiangVien { get; set; }
        public string? BaiThi { get; set; }
        public string? HinhThuc { get; set; }
        public string? PhongThi { get; set; }
    }

    public class DanhSachBuoiHocTuanResponseDTO
    {
        public string TuNgay { get; set; } = string.Empty;
        public string DenNgay { get; set; } = string.Empty;
        public List<BuoiHocDanhSachDTO> DanhSachBuoiHoc { get; set; } = new List<BuoiHocDanhSachDTO>();
    }

    public class BuoiHocDanhSachDTO
    {
        public int Id { get; set; }
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }
        public string? TrangThai { get; set; }
        public string? ThoiGian { get; set; }
        public string? ChuKyTuan { get; set; }
        public string? DiaDiem { get; set; }
        public string? GiangVien { get; set; }
        public string? EmailGiangVien { get; set; }
        public string? LoaiBuoiHoc { get; set; }
    }

    public class BuoiHocHomNayResponseDTO
    {
        public string Ngay { get; set; } = string.Empty;
        public string Thu { get; set; } = string.Empty;
        public List<BuoiHocTKBItemDTO> BuoiHoc { get; set; } = new List<BuoiHocTKBItemDTO>();
        public int TongSoBuoi { get; set; }
    }
}