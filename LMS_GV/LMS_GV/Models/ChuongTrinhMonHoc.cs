using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class ChuongTrinhMonHoc
{
    public int ChuongTrinhMonHocId { get; set; }

    public int ChuongTrinhDaoTaoId { get; set; }

    public int MonHocId { get; set; }

    public int? HocKy { get; set; }

    public bool? DaBatBuoc { get; set; }

    public int? SoTinChi { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ChuongTrinhDaoTao ChuongTrinhDaoTao { get; set; } = null!;

    public virtual MonHoc MonHoc { get; set; } = null!;
}
