using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class NguoiDung
{
    public int NguoiDungId { get; set; }
    public string TenDangNhap { get; set; } = null!;
    public string? HashMatKhau { get; set; }
    public string? Email { get; set; }
    public string? HoTen { get; set; }
    public string? DangnhapGoogle { get; set; }
    public bool? EmailGoogleVerified { get; set; }
    public string? Avatar { get; set; }
    public DateTime? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public byte? TrangThai { get; set; }
    public int? VaiTroId { get; set; }
    public DateTime? LanDangNhapCuoi { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BaiHoc> BaiHocs { get; set; } = new List<BaiHoc>();

    public virtual ICollection<BaiKiemTra> BaiKiemTraCreatedByNavigations { get; set; } = new List<BaiKiemTra>();

    public virtual ICollection<BaiKiemTra> BaiKiemTraGiangViens { get; set; } = new List<BaiKiemTra>();

    public virtual ICollection<BaoCao> BaoCaos { get; set; } = new List<BaoCao>();

    public virtual ICollection<GiangVien> GiangViens { get; set; } = new List<GiangVien>();

    public virtual ICollection<HoSoSinhVien> HoSoSinhViens { get; set; } = new List<HoSoSinhVien>();

    public virtual ICollection<LichSuKien> LichSuKiens { get; set; } = new List<LichSuKien>();

    public virtual ICollection<LichSuSuaDiem> LichSuSuaDiems { get; set; } = new List<LichSuSuaDiem>();

    public virtual ICollection<NhatKyHeThong> NhatKyHeThongs { get; set; } = new List<NhatKyHeThong>();

    public virtual ICollection<ThongBaoNguoiDung> ThongBaoNguoiDungs { get; set; } = new List<ThongBaoNguoiDung>();

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();

    public virtual VaiTro? VaiTro { get; set; }


}
