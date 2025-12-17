using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class BaiKiemTra
{
    public int BaiKiemTraId { get; set; }

    public int LopHocId { get; set; }

    public int? MonHocId { get; set; }

    public int? GiangVienId { get; set; }

    public string Loai { get; set; } = null!;

    public string TieuDe { get; set; } = null!;

    public string? MoTa { get; set; }

    public DateTime? NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public int? ThoiGianLamBai { get; set; }

    public int? SoCau { get; set; }

    public decimal? DiemToiDa { get; set; }

    public bool? IsRandom { get; set; }

    public bool? ChoPhepLamLai { get; set; }

    public int? SoLanLamToiDa { get; set; }

    public byte? TrangThai { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BaiNop> BaiNops { get; set; } = new List<BaiNop>();

    public virtual ICollection<CauHoi> CauHois { get; set; } = new List<CauHoi>();

    public virtual NguoiDung? CreatedByNavigation { get; set; }

    public virtual NguoiDung? GiangVien { get; set; }

    public virtual ICollection<KetQuaKiemTra> KetQuaKiemTras { get; set; } = new List<KetQuaKiemTra>();

    public virtual LopHoc LopHoc { get; set; } = null!;

    public virtual MonHoc? MonHoc { get; set; }

    public virtual ICollection<TienDo> TienDos { get; set; } = new List<TienDo>();
}
