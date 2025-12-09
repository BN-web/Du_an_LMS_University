using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class Nganh
{
    public int NganhId { get; set; }

    public string? MaNganh { get; set; }

    public string TenNganh { get; set; } = null!;

    public int? KhoaId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; } = new List<ChuongTrinhDaoTao>();

    public virtual Khoa? Khoa { get; set; }

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
