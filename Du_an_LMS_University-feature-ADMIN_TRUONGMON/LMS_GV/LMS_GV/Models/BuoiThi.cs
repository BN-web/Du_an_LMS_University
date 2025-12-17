using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class BuoiThi
{
    public int BuoiThiId { get; set; }

    public int LopHocId { get; set; }

    public int? PhongHocId { get; set; }

    public DateTime NgayThi { get; set; }

    public string Thứ { get; set; } = null!;

    public TimeOnly? GioBatDau { get; set; }

    public TimeOnly? GioKetThuc { get; set; }

    public string? HinhThuc { get; set; }

    public int? GiamThiId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public virtual GiangVien? GiamThi { get; set; }

    public virtual LopHoc LopHoc { get; set; } = null!;

    public virtual PhongHoc? PhongHoc { get; set; }
}
