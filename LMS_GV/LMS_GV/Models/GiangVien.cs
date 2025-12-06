using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class GiangVien
{
    public int GiangVienId { get; set; }

    public string? Ms { get; set; }

    public int NguoiDungId { get; set; }

    public string? ChucVu { get; set; }

    public int? KhoaId { get; set; }

    public string? HocVi { get; set; }

    public string? ChuyenMon { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BuoiThi> BuoiThis { get; set; } = new List<BuoiThi>();

    public virtual ICollection<GiangVienLop> GiangVienLops { get; set; } = new List<GiangVienLop>();

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    public virtual NguoiDung NguoiDung { get; set; } = null!;
}
