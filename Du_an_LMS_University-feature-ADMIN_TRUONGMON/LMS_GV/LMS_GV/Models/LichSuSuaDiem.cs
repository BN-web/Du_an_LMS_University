using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class LichSuSuaDiem
{
    public int LichSuSuaDiemId { get; set; }

    public int DiemId { get; set; }

    public int? NguoiDungId { get; set; }

    public decimal? DiemLucDau { get; set; }

    public decimal? DiemMoi { get; set; }

    public string? LyDo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Diem Diem { get; set; } = null!;

    public virtual NguoiDung? NguoiDung { get; set; }
}
