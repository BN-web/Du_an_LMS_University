using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class DiemDanh
{
    public int DiemDanhId { get; set; }

    public int LopHocId { get; set; }

    public int? BuoiHocId { get; set; }

    public int? BuoiThiId { get; set; }

    public DateTime? Ngay { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual BuoiHoc? BuoiHoc { get; set; }

    public virtual BuoiThi? BuoiThi { get; set; }

    public virtual ICollection<DiemDanhChiTiet> DiemDanhChiTiets { get; set; } = new List<DiemDanhChiTiet>();

    public virtual LopHoc LopHoc { get; set; } = null!;
}
