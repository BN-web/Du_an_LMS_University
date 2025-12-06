using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class ThongKeHocTap
{
    public int ThongKeHocTapId { get; set; }

    public string? Ten { get; set; }

    public double? GiaTri { get; set; }

    public int? SinhVienId { get; set; }

    public int? LopHocId { get; set; }

    public int? HocKyId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual HocKy? HocKy { get; set; }

    public virtual LopHoc? LopHoc { get; set; }

    public virtual HoSoSinhVien? SinhVien { get; set; }
}
