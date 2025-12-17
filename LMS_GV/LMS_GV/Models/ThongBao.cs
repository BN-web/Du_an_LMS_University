using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class ThongBao
{
    public int ThongBaoId { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? NoiDung { get; set; }

    public int? NguoiDangId { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    public virtual NguoiDung? NguoiDang { get; set; }

    public virtual ICollection<ThongBaoNguoiDung> ThongBaoNguoiDungs { get; set; } = new List<ThongBaoNguoiDung>();
}
