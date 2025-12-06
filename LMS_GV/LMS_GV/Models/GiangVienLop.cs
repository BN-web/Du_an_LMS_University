using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class GiangVienLop
{
    public int GiangVienLopId { get; set; }

    public int GiangVienId { get; set; }

    public int LopHocId { get; set; }

    public string? TenGiangVien { get; set; }

    public DateTime? NgayPhanCong { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual GiangVien GiangVien { get; set; } = null!;

    public virtual LopHoc LopHoc { get; set; } = null!;
}
