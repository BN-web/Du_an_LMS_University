using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class BaiHoc
{
    public int BaiHocId { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? NoiDung { get; set; }

    public string? LoaiBaiHoc { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BaiHocLop> BaiHocLops { get; set; } = new List<BaiHocLop>();

    public virtual NguoiDung? CreatedByNavigation { get; set; }

    public virtual ICollection<File> Files { get; set; } = new List<File>();
}
