using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class Diem
{
    public int DiemId { get; set; }

    public int SinhVienId { get; set; }

    public int LopHocId { get; set; }

    public int HocKyId { get; set; }

    public decimal? DiemTrungBinhMon { get; set; }

    public string? DiemChu { get; set; }

    public decimal? Gpamon { get; set; }

    public decimal? HeSoMon { get; set; }

    public int? SoTinChi { get; set; }

    public byte? TrangThai { get; set; }

    public string? LyDoRoiMon { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual HocKy HocKy { get; set; } = null!;

    public virtual ICollection<LichSuSuaDiem> LichSuSuaDiems { get; set; } = new List<LichSuSuaDiem>();

    public virtual LopHoc LopHoc { get; set; } = null!;

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;
}
