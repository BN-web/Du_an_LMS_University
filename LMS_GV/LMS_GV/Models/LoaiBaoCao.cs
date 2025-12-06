using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class LoaiBaoCao
{
    public int LoaiBaoCaoId { get; set; }

    public string TenLoai { get; set; } = null!;

    public string? MoTa { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BaoCao> BaoCaos { get; set; } = new List<BaoCao>();
}
