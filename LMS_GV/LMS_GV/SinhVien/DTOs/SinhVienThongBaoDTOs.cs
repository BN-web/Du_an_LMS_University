using System;

namespace LMS_GV.DTOs.SinhVien
{
    public class ThongBaoListDTO
    {
        public int Id { get; set; }
        public string TieuDe { get; set; }
        public string NgayGui { get; set; }
        public string NoiDungRutGon { get; set; }
        public bool DaDoc { get; set; }
        public bool CoTheXemChiTiet { get; set; }
    }

    public class ThongBaoDetailDTO
    {
        public int Id { get; set; }
        public string TieuDe { get; set; }
        public string NgayGui { get; set; }
        public string NoiDung { get; set; }
        public bool DaDoc { get; set; }
        public string NguoiGui { get; set; }
        public string LoaiThongBao { get; set; }
    }

    public class ThongBaoChuaDocDTO
    {
        public int SoThongBaoChuaDoc { get; set; }
        public ThongBaoMoiNhatDTO ThongBaoMoiNhat { get; set; }
    }

    public class ThongBaoMoiNhatDTO
    {
        public int Id { get; set; }
        public string TieuDe { get; set; }
        public string NgayGui { get; set; }
    }

    public class ThongBaoFilterDTO
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public bool? DaDoc { get; set; }
    }

    public class ThongBaoResponseDTO
    {
        public List<ThongBaoListDTO> ThongBao { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
    }
}