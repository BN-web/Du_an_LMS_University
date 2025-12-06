using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class Khoa
{
    public int KhoaId { get; set; }

    public string? MaKhoa { get; set; }

    public string TenKhoa { get; set; } = null!;

    public string? MoTa { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    public virtual ICollection<Nganh> Nganhs { get; set; } = new List<Nganh>();
}
