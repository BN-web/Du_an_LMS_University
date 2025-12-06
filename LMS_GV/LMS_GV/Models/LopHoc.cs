using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class LopHoc
{
    public int LopHocId { get; set; }

    public string? MaLop { get; set; }

    public string? TenLop { get; set; }

    public int MonHocId { get; set; }

    public int HocKyId { get; set; }

    public int? KhoaTuyenSinhId { get; set; }

    public int? KhoaId { get; set; }

    public int? NganhId { get; set; }

    public int? SoTinChi { get; set; }

    public int? SoTiet { get; set; }

    public int? SoBuoiVangChoPhep { get; set; }

    public int? GiangVienId { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public int? SiSo { get; set; }

    public int? SiSoToiDa { get; set; }

    public byte? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BaiHocLop> BaiHocLops { get; set; } = new List<BaiHocLop>();

    public virtual ICollection<BaiKiemTra> BaiKiemTras { get; set; } = new List<BaiKiemTra>();

    public virtual ICollection<BuoiHoc> BuoiHocs { get; set; } = new List<BuoiHoc>();

    public virtual ICollection<BuoiThi> BuoiThis { get; set; } = new List<BuoiThi>();

    public virtual ICollection<DiemChuyenCan> DiemChuyenCans { get; set; } = new List<DiemChuyenCan>();

    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public virtual ICollection<DiemThanhPhan> DiemThanhPhans { get; set; } = new List<DiemThanhPhan>();

    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();

    public virtual GiangVien? GiangVien { get; set; }

    public virtual ICollection<GiangVienLop> GiangVienLops { get; set; } = new List<GiangVienLop>();

    public virtual HocKy HocKy { get; set; } = null!;

    public virtual Khoa? Khoa { get; set; }

    public virtual KhoaTuyenSinh? KhoaTuyenSinh { get; set; }

    public virtual ICollection<LichSuKien> LichSuKiens { get; set; } = new List<LichSuKien>();

    public virtual MonHoc MonHoc { get; set; } = null!;

    public virtual Nganh? Nganh { get; set; }

    public virtual ICollection<SinhVienLop> SinhVienLops { get; set; } = new List<SinhVienLop>();

    public virtual ICollection<ThanhPhanDiem> ThanhPhanDiems { get; set; } = new List<ThanhPhanDiem>();

    public virtual ICollection<ThongKeHocTap> ThongKeHocTaps { get; set; } = new List<ThongKeHocTap>();
}
