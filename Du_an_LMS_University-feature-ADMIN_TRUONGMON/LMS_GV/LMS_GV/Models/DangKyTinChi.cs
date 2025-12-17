using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class DangKyTinChi
{
    public int DangKyTinChiId { get; set; }

    public int SinhVienId { get; set; }

    public int MonHocId { get; set; }

    public int HocKyId { get; set; }

    public DateTime? NgayDangKy { get; set; }

    public byte? TrangThai { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual HocKy HocKy { get; set; } = null!;

    public virtual MonHoc MonHoc { get; set; } = null!;

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;
}
