using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class KhoaTuyenSinh
{
    public int KhoaTuyenSinhId { get; set; }

    public string? MaKhoaTuyenSinh { get; set; }

    public string TenKhoaTuyenSinh { get; set; } = null!;

    public string? MoTa { get; set; }

    public int? NamBatDau { get; set; }

    public int? NamKetThuc { get; set; }

    public byte? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; } = new List<ChuongTrinhDaoTao>();

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
