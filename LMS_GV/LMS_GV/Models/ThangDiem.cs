using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class ThangDiem
{
    public int ThangDiemId { get; set; }

    public string? DiemChu { get; set; }

    public decimal? DiemMin { get; set; }

    public decimal? DiemMax { get; set; }

    public DateTime? CreatedAt { get; set; }
}
