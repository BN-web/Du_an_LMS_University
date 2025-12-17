using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class ThongBaoNguoiDung
{
    public int ThongBaoNguoiDungId { get; set; }

    public int ThongBaoId { get; set; }

    public int NguoiDungId { get; set; }

    public bool? DaDoc { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual NguoiDung NguoiDung { get; set; } = null!;

    public virtual ThongBao ThongBao { get; set; } = null!;
}
