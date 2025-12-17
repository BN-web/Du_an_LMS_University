namespace LMS_GV.SinhVien.DTOs
{
    public class LopHocItemDTO
    {
        public int LopHocId { get; set; }
        public string? MaLop { get; set; }
        public string? TenLop { get; set; }
        public string? GiangVien { get; set; }
        public string? EmailGiangVien { get; set; }
        public string? Nganh { get; set; }
        public int? SoTinChi { get; set; }
        public string? HocKy { get; set; }
    }

    public class BaiTapItemDTO
    {
        public int BaiTapId { get; set; }
        public string? TieuDe { get; set; }
        public string? MonHoc { get; set; }
        public string? LopHoc { get; set; } // Thêm mới
        public string? GiangVien { get; set; }
        public string? ThoiGianNop { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? LoaiBai { get; set; }
        public decimal? DiemToiDa { get; set; }
        public decimal? DiemDatDuoc { get; set; }
        public string? ThoiGianNopBai { get; set; }
        public int? ThoiGianLamBai { get; set; }
        public int? ThoiGianLamBaiToiDa { get; set; }
        public bool ChoPhepLamLai { get; set; }
    }

    public class CauHoiDTO
    {
        public int CauHoiId { get; set; }
        public string? NoiDung { get; set; }
        public string? Loai { get; set; }
        public decimal? Diem { get; set; }
        public List<TuyChonCauHoiDTO> TuyChon { get; set; } = new List<TuyChonCauHoiDTO>();
    }

    public class TuyChonCauHoiDTO
    {
        public string? MaLuaChon { get; set; }
        public string? NoiDung { get; set; }
        public bool LaDapAn { get; set; }
    }

    public class BatDauBaiTapResponseDTO
    {
        public int BaiNopId { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public long? ThoiGianConLai { get; set; }
    }

    public class TienDoRequestDTO
    {
        public int BaiNopId { get; set; }
        public int SoCauDaLam { get; set; }
        public List<CauTraLoiRequestDTO>? CauTraLoi { get; set; }
    }

    public class CauTraLoiRequestDTO
    {
        public int CauHoiId { get; set; }
        public List<string>? LuaChonSinhVien { get; set; }
    }

    public class NopBaiRequestDTO
    {
        public int BaiNopId { get; set; }
        public List<CauTraLoiRequestDTO>? CauTraLoi { get; set; }
    }

    public class NopBaiResponseDTO
    {
        public int BaiNopId { get; set; }
        public DateTime? ThoiGianNop { get; set; }
        public int SoCauDaTraLoi { get; set; }
        public int TongSoCau { get; set; }
    }

    public class KetQuaBaiTapResponseDTO
    {
        public int BaiNopId { get; set; }
        public string? TieuDe { get; set; }
        public decimal? DiemDatDuoc { get; set; }
        public decimal? DiemToiDa { get; set; }
        public int? ThoiGianLamBai { get; set; }
        public int? ThoiGianLamBaiToiDa { get; set; }
        public int? SoLanLam { get; set; }
        public bool ChoPhepLamLai { get; set; }
        public List<ChiTietCauTraLoiDTO>? ChiTietCauTraLoi { get; set; }
    }

    public class ChiTietCauTraLoiDTO
    {
        public int CauHoiId { get; set; }
        public string? NoiDungCauHoi { get; set; }
        public List<string>? LuaChonSinhVien { get; set; }
        public List<string>? DapAnDung { get; set; }
        public decimal? Diem { get; set; }
        public bool Dung { get; set; }
    }

    public class BangDiemItemDTO
    {
        public int LopHocId { get; set; } // Thêm mới
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }

        public string? HocKy { get; set; } // Thêm mới
        public decimal? DiemTrungBinh { get; set; }
        public decimal? GpaMon { get; set; }
        public int? SoTinChi { get; set; }
        public string? TrangThai { get; set; }
        public string? DiemChu { get; set; }
        public decimal? HeSo { get; set; } // Thêm mới


    }

    public class BangDiemChiTietDTO
    {
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }
        public string? TrangThai { get; set; }
        public int? SoBuoiVang { get; set; }
        public int? SoTinChi { get; set; }
        public decimal? HeSo { get; set; }
        public decimal? GpaMon { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public string? DiemChu { get; set; }
        public decimal? DiemChuyenCan { get; set; }
        public decimal? DiemBaiTap { get; set; }
        public decimal? DiemGiuaKy { get; set; }
        public decimal? DiemCuoiKy { get; set; }
    }

    public class TaiLieuItemDTO
    {
        public int TaiLieuId { get; set; }
        public string? TieuDe { get; set; }
        public string? Loai { get; set; }
        public string? MonHoc { get; set; }
        public string? ThoiGianDang { get; set; }
        public string? GiangVien { get; set; }
        public string? DuongDan { get; set; }
        public long? KichThuoc { get; set; }
        public string? NguoiDang { get; set; }
    }

    public class BaoCaoGianLanRequestDTO
    {
        public int BaiNopId { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public string? ChiTiet { get; set; }
    }

    // DTO cho response của GetAllBaiTap
    public class AllBaiTapResponseDTO
    {
        public List<BaiTapItemDTO> BaiTap { get; set; } = new List<BaiTapItemDTO>();
        public int TongSoBaiTap { get; set; }
        public int SoBaiDaNop { get; set; }
        public int SoBaiChuaNop { get; set; }
    }

    // DTO cho response của GetAllBangDiem
    public class AllBangDiemResponseDTO
    {
        public List<BangDiemItemDTO> BangDiem { get; set; } = new List<BangDiemItemDTO>();
        public int TongSoMon { get; set; }
        public int? TongTinChi { get; set; }
        public decimal? GpaTong { get; set; }
        public decimal? DiemTrungBinhTong { get; set; }
    }
}