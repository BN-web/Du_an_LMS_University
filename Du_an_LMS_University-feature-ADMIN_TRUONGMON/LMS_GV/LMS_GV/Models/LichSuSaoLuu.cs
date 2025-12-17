using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class LichSuSaoLuu
{
    public int LichSuSaoLuuId { get; set; }

    public string? DuongDan { get; set; }

    public string? GhiChu { get; set; }

    public DateTime? CreatedAt { get; set; }
}
