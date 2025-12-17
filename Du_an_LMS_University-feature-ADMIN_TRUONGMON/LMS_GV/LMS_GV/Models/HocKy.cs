using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class HocKy
{
    public int HocKyId { get; set; }

    public string? NamHoc { get; set; }

    public string? KiHoc { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DangKyTinChi> DangKyTinChis { get; set; } = new List<DangKyTinChi>();

    public virtual ICollection<DiemChuyenCan> DiemChuyenCans { get; set; } = new List<DiemChuyenCan>();

    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    public virtual ICollection<ThongKeHocTap> ThongKeHocTaps { get; set; } = new List<ThongKeHocTap>();
}
