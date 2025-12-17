using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class PhongHoc
{
    public int PhongHocId { get; set; }

    public string? MaPhong { get; set; }

    public string? TenPhong { get; set; }

    public int? SucChua { get; set; }

    public string? DiaChi { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BuoiHoc> BuoiHocs { get; set; } = new List<BuoiHoc>();

    public virtual ICollection<BuoiThi> BuoiThis { get; set; } = new List<BuoiThi>();
}
