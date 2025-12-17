using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class TienDo
{
    public int TienDoId { get; set; }

    public int SinhVienId { get; set; }

    public int BaiKiemTraId { get; set; }

    public int? SoCauDaLam { get; set; }

    public byte? TrangThai { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BaiKiemTra BaiKiemTra { get; set; } = null!;

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;
}
