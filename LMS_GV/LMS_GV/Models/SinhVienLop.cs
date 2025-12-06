using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class SinhVienLop
{
    public int SinhVienLopId { get; set; }

    public int SinhVienId { get; set; }

    public int LopHocId { get; set; }

    public DateTime? NgayVao { get; set; }

    public string? TinhTrang { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual LopHoc LopHoc { get; set; } = null!;

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;
}
