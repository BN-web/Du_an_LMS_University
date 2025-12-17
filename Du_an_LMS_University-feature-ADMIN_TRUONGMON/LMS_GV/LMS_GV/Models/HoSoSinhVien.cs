using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class HoSoSinhVien
{
    public int SinhVienId { get; set; }

    public string? Mssv { get; set; }

    public int NguoiDungId { get; set; }

    public string? ThoiGianDaoTao { get; set; }

    public int? TongTinChi { get; set; }

    public decimal? Gpa { get; set; }

    public int? NganhId { get; set; }

    public int? KhoaTuyenSinhId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BaiNop> BaiNops { get; set; } = new List<BaiNop>();

    public virtual ICollection<DangKyTinChi> DangKyTinChis { get; set; } = new List<DangKyTinChi>();

    public virtual ICollection<DiemChuyenCan> DiemChuyenCans { get; set; } = new List<DiemChuyenCan>();

    public virtual ICollection<DiemDanhChiTiet> DiemDanhChiTiets { get; set; } = new List<DiemDanhChiTiet>();

    public virtual ICollection<DiemThanhPhan> DiemThanhPhans { get; set; } = new List<DiemThanhPhan>();

    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();

    public virtual ICollection<KetQuaKiemTra> KetQuaKiemTras { get; set; } = new List<KetQuaKiemTra>();

    public virtual NguoiDung NguoiDung { get; set; } = null!;

    public virtual Nganh? Nganh { get; set; }

    public virtual KhoaTuyenSinh? KhoaTuyenSinh { get; set; }

    public virtual ICollection<SinhVienLop> SinhVienLops { get; set; } = new List<SinhVienLop>();

    public virtual ICollection<ThongKeHocTap> ThongKeHocTaps { get; set; } = new List<ThongKeHocTap>();

    public virtual ICollection<TienDo> TienDos { get; set; } = new List<TienDo>();
}
