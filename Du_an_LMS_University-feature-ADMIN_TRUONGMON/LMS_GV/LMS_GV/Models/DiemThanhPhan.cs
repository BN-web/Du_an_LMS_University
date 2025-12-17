using System;
using System.Collections.Generic;

namespace LMS_GV.Models;

public partial class DiemThanhPhan
{
    public int DiemThanhPhanId { get; set; }

    public int SinhVienId { get; set; }

    public int LopHocId { get; set; }

    public int ThanhPhanDiemId { get; set; }

    public decimal? Diem { get; set; }

    public string? GhiChu { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual LopHoc LopHoc { get; set; } = null!;

    public virtual HoSoSinhVien SinhVien { get; set; } = null!;

    public virtual ThanhPhanDiem ThanhPhanDiem { get; set; } = null!;
}
