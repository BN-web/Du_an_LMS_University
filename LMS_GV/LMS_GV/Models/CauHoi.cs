using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class CauHoi
{
    public int CauHoiId { get; set; }

    public int BaiKiemTraId { get; set; }

    public string NoiDung { get; set; } = null!;

    public string? Loai { get; set; }

    public decimal? Diem { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BaiKiemTra BaiKiemTra { get; set; } = null!;

    public virtual ICollection<ChiTietCauTraLoi> ChiTietCauTraLois { get; set; } = new List<ChiTietCauTraLoi>();

    public virtual ICollection<TuyChonCauHoi> TuyChonCauHois { get; set; } = new List<TuyChonCauHoi>();
}
