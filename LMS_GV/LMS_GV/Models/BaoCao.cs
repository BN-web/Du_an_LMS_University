using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class BaoCao
{
    public int BaoCaoId { get; set; }

    public int? LoaiBaoCaoId { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? FilePath { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual NguoiDung? CreatedByNavigation { get; set; }

    public virtual LoaiBaoCao? LoaiBaoCao { get; set; }
}
