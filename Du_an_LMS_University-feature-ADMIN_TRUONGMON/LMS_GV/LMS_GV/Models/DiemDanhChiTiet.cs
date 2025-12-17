using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class DiemDanhChiTiet
{
    public int DiemDanhChiTietId { get; set; }

    public int DiemDanhId { get; set; }

    public int SinhVienId { get; set; }

    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public virtual DiemDanh DiemDanh { get; set; } = null!;

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;
}
