using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class ThanhPhanDiem
{
    public int ThanhPhanDiemId { get; set; }

    public int LopHocId { get; set; }

    public string Ten { get; set; } = null!;

    public decimal? HeSo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DiemThanhPhan> DiemThanhPhans { get; set; } = new List<DiemThanhPhan>();

    public virtual LopHoc LopHoc { get; set; } = null!;
}
