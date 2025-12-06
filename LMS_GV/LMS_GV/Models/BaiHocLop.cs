using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class BaiHocLop
{
    public int BaiHocLopId { get; set; }

    public int BaiHocId { get; set; }

    public int LopHocId { get; set; }

    public int? ViTri { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BaiHoc BaiHoc { get; set; } = null!;

    public virtual LopHoc LopHoc { get; set; } = null!;
}
