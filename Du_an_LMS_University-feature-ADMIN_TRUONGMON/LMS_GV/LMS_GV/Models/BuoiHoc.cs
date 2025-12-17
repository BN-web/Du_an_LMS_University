using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class BuoiHoc
{
    public int BuoiHocId { get; set; }

    public int LopHocId { get; set; }

    public int? PhongHocId { get; set; }

    public string Thứ { get; set; } = null!;

    public DateTime ThoiGianBatDau { get; set; }

    public DateTime ThoiGianKetThuc { get; set; }

    public string? LoaiBuoiHoc { get; set; }

    public byte? TrangThai { get; set; }

    public int SoBuoi { get; set; }

    public string? GhiChu { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public virtual ICollection<LichSuKien> LichSuKiens { get; set; } = new List<LichSuKien>();

    public virtual LopHoc LopHoc { get; set; } = null!;

    public virtual PhongHoc? PhongHoc { get; set; }
}
