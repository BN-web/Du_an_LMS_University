using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class ChuongTrinhDaoTao
{
    public int ChuongTrinhDaoTaoId { get; set; }

    public int NganhId { get; set; }

    public int KhoaTuyenSinhId { get; set; }

    public int? TongTinCanBo { get; set; }

    public string? GhiChu { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChuongTrinhMonHoc> ChuongTrinhMonHocs { get; set; } = new List<ChuongTrinhMonHoc>();

    public virtual KhoaTuyenSinh KhoaTuyenSinh { get; set; } = null!;

    public virtual Nganh Nganh { get; set; } = null!;
}
