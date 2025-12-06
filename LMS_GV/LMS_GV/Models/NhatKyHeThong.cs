using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class NhatKyHeThong
{
    public int NhatKyHeThongId { get; set; }

    public int? NguoiDungId { get; set; }

    public string? HanhDong { get; set; }

    public string? ThamChieu { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual NguoiDung? NguoiDung { get; set; }
}
