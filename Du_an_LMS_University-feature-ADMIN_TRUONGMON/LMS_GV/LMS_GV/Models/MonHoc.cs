using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class MonHoc
{
    public int MonHocId { get; set; }

    public string? MaMon { get; set; }

    public string TenMon { get; set; } = null!;

    public int? SoTinChi { get; set; }

    public string? MoTa { get; set; }

    public byte? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BaiKiemTra> BaiKiemTras { get; set; } = new List<BaiKiemTra>();

    public virtual ICollection<ChuongTrinhMonHoc> ChuongTrinhMonHocs { get; set; } = new List<ChuongTrinhMonHoc>();

    public virtual ICollection<DangKyTinChi> DangKyTinChis { get; set; } = new List<DangKyTinChi>();

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
