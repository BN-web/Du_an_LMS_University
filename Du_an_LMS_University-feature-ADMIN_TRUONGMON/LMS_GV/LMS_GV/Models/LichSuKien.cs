using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class LichSuKien
{
    public int LichSuKienId { get; set; }

    public int? LopHocId { get; set; }

    public int? BuoiHocId { get; set; }

    public int? NguoiTaoId { get; set; }

    public string? TieuDe { get; set; }

    public string? Loai { get; set; }

    public DateTime? NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public string? MoTa { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BuoiHoc? BuoiHoc { get; set; }

    public virtual LopHoc? LopHoc { get; set; }

    public virtual NguoiDung? NguoiTao { get; set; }
}
