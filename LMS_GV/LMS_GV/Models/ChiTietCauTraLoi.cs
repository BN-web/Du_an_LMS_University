using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class ChiTietCauTraLoi
{
    public int ChiTietCauTraLoiId { get; set; }

    public int BaiNopId { get; set; }

    public int CauHoiId { get; set; }

    public string? LuaChonSinhVien { get; set; }

    public decimal? Diem { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BaiNop BaiNop { get; set; } = null!;

    public virtual CauHoi CauHoi { get; set; } = null!;
}
