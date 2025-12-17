using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class KetQuaKiemTra
{
    public int KetQuaKiemTraId { get; set; }

    public int BaiKiemTraId { get; set; }

    public int SinhVienId { get; set; }

    public decimal? TongDiem { get; set; }

    public int? ThoiGianLamBai { get; set; }

    public bool? IsAutoGraded { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BaiKiemTra BaiKiemTra { get; set; } = null!;

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;
}
