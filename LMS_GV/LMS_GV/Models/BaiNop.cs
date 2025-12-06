using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class BaiNop
{
    public int BaiNopId { get; set; }

    public int BaiKiemTraId { get; set; }

    public int SinhVienId { get; set; }

    public DateTime? NgayNop { get; set; }

    public decimal? TongDiem { get; set; }

    public byte? TrangThai { get; set; }

    public int? LanLam { get; set; }

    public int? ThoiGianHoanThanh { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BaiKiemTra BaiKiemTra { get; set; } = null!;

    public virtual ICollection<ChiTietCauTraLoi> ChiTietCauTraLois { get; set; } = new List<ChiTietCauTraLoi>();

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;
}
