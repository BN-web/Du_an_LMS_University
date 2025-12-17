using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class DiemChuyenCan
{
    public int DiemChuyenCanId { get; set; }

    public int SinhVienId { get; set; }

    public int LopHocId { get; set; }

    public int HocKyId { get; set; }

    public decimal? Diem { get; set; }

    public string? GhiChu { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual HocKy HocKy { get; set; } = null!;

    public virtual LopHoc LopHoc { get; set; } = null!;

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;
}
