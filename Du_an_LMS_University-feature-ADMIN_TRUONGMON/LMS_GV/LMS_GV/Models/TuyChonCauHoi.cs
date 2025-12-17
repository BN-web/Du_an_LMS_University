using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class TuyChonCauHoi
{
    public int TuyChonCauHoiId { get; set; }

    public int CauHoiId { get; set; }

    public string MaLuaChon { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public bool LaDapAn { get; set; }

    public int? ThuTu { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual CauHoi CauHoi { get; set; } = null!;
}
