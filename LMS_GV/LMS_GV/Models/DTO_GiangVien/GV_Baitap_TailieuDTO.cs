namespace LMS_GV.Models.DTO_GiangVien
{
    //----Trang 1------//
    public class BaiTapGiangVienDto
    {
        public int BaiKiemTraId { get; set; }
        public string TieuDe { get; set; }
        public string TenMon { get; set; }
        public string MaLop { get; set; }

        public string NgayTao { get; set; }  // Thay DateTime thành string
        public string HanNop { get; set; }   // Thay DateTime? thành string
        public string Loai { get; set; } // Bài tập / Bài kiểm tra / Bài thi

        // Tính toán
        public int TongSinhVien { get; set; }
        public int DaNop { get; set; }
        public int ChuaNop { get; set; }
        public double TienDoPhanTram { get; set; }

        public int SoLanLamBai { get; set; } // Số lần sinh viên đã làm
    }

    //--API tạo bài tập--//
    public class TaoBaiTapRequestDto
    {
        public string TieuDe { get; set; }
        public string Loai { get; set; } // Bài tập / Bài kiểm tra / Bài thi

        // Thay DateTime? thành string để parse định dạng dd/MM/yyyy HH:mm
        public string HanNop { get; set; }
        public int? SoLanLamToiDa { get; set; }
        public int? ThoiGianLamBai { get; set; } // phút

        public int LopHocId { get; set; } // chọn lớp

        public List<CauHoiTaoDto> CauHois { get; set; }
    }

    public class CauHoiTaoDto
    {
        public string NoiDung { get; set; }

        public List<DapAnTaoDto> DapAns { get; set; }

        public int DapAnDung { get; set; } // index đáp án đúng, ví dụ: 0=A,1=B,...
    }

    public class DapAnTaoDto
    {
        public string NoiDung { get; set; }
        public int ThuTu { get; set; }
    }


    // DTO thay đổi thứ tự dáp án trong câu hỏi
    public class ThayDoiThuTuDapAnDto
    {
        public int CauHoiId { get; set; }
        public List<DapAnThuTuDto> ThuTus { get; set; }
    }

    public class DapAnThuTuDto
    {
        public int TuyChonCauHoiId { get; set; }
        public int ThuTuMoi { get; set; }
    }


    //--API Xem chi tiết bài tập/bài kiểm tra/bài thi--//

    //Thông tin tổng quan bài kiểm tra
    public class BaiKiemTraChiTietDto
    {
        public int BaiKiemTraId { get; set; }
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public string Loai { get; set; }
        public string TenMonHoc { get; set; }
        public string HanNop { get; set; } // giờ:phút ngày/tháng/năm
        public int? ThoiGianLamBai { get; set; }
        public int TongDaNop { get; set; }
        public int TongCauHoi { get; set; }

        // Thêm thuộc tính số lần làm bài tối đa
        public int? SoLanLamToiDa { get; set; }
    }


    //Sinh viên đã nộp bài
    public class SinhVienNopBaiDto
    {
        public string MSSV { get; set; }
        public string HoTen { get; set; }
        public string ThoiGianNop { get; set; }
        public int LanLam { get; set; }
        public string TrangThai { get; set; }
        public decimal? Diem { get; set; }
    }

    // Tổng quan bài làm của sinh viên
    public class BaiLamSinhVienDto
    {
        public string MSSV { get; set; }
        public string HoTen { get; set; }
        public DateTime? HanNop { get; set; }
        public decimal? TongDiemBaiKiemTra { get; set; }
        public int TongSoCauHoi { get; set; }
        public int TongCauDung { get; set; } // tự tính
        public double PhanTramDung { get; set; } // % độ chính xác
    }

    // DTO thông tin tổng quan bài kiểm tra
    public class BaiKiemTraCauHoiDto
    {
        public int BaiKiemTraId { get; set; }
        public string TieuDe { get; set; }
        public int TongSoCauHoi { get; set; }
        public List<CauHoiChiTietDto> CauHois { get; set; } = new List<CauHoiChiTietDto>();
    }

    // DTO chi tiết từng câu hỏi
    public class CauHoiChiTietDto
    {
        public int CauHoiId { get; set; }
        public string NoiDung { get; set; }
        public decimal? Diem { get; set; }
        public List<DapAnDto> DapAns { get; set; } = new List<DapAnDto>();
        public List<DapAnDto> DapAnDung => DapAns.Where(x => x.LaDapAn).ToList();
    }

    // DTO đáp án
    public class DapAnDto
    {
        public string MaLuaChon { get; set; }
        public string NoiDung { get; set; }
        public bool LaDapAn { get; set; }
    }

    // Chi tiết câu hỏi của sinh viên
    public class ChiTietCauHoiSinhVienDto
    {
        public string NoiDungCauHoi { get; set; }
        public decimal? DiemSinhVien { get; set; }
        public decimal? DiemToiDa { get; set; }
        public string MaLuaChon { get; set; }
        public string NoiDungLuaChon { get; set; }
    }

    ///------------Trang tài liệu-----------------///

    // DTO danh sách tài liệu của giảng viên
    public class TaiLieuDto
    {
        public string TenMonHoc { get; set; }
        public int TongTaiLieuLop { get; set; }
        public string TenBaiHoc { get; set; }
        public string LoaiBaiHoc { get; set; }
        public string TenFile { get; set; }
        public string DuongDan { get; set; }
        public string NgayUpload { get; set; } // dd/MM/yyyy
        public long KichThuoc { get; set; } // byte
    }

    // DTO tải tài liệu
    public class UploadTaiLieuDto
    {
        public int BaiHocId { get; set; }
        public int LopHocId { get; set; }
        public string MoTaBaiHoc { get; set; }
        public IFormFile File { get; set; } // upload file từ client
    }

    public class TaiFileRequestDto
    {
        public int FileId { get; set; }
    }

}
