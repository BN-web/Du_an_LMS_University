using System.ComponentModel.DataAnnotations;

namespace LMS_GV.Models.DTO_GiangVien
{
    // Dùng để trả dữ liệu lịch dạy theo dạng calendar (tuần view)

    public class LichDayTuanRequest
    {
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime EndDate { get; set; }
    }

    public class LichDayTuanDto
    {
        public int BuoiHocId { get; set; }

        public string TenMon { get; set; }      // Tên môn học
        public string Lop { get; set; }         // Mã lớp
        public string GiangVien { get; set; }   // Tên giảng viên

        public DateTime Ngay { get; set; }      // Ngày học (dùng để render theo cột Thứ)
        public string Thu { get; set; }         // Thứ trong tuần (Thứ 2, Thứ 3,...)

        public string GioBatDau { get; set; }    // HH:mm
        public string GioKetThuc { get; set; }   // HH:mm

        public string Phong { get; set; }        // Tên phòng (nếu offline)
        public string DiaDiem { get; set; }      // Cơ sở – Tòa – Phòng

        public string TrangThai { get; set; }    // Bình thường | Đã hủy | Đổi giờ | Đổi phòng | Online
        public string LoaiBuoiHoc { get; set; }  // giang bai / lab / tutorial

        public int SoLuongSinhVien { get; set; }
    }


    public class BuoiHocChiTietDto
    {
        public int BuoiHocId { get; set; }
        public string MonHoc { get; set; }
        public string Lop { get; set; }
        public string GiangVien { get; set; }

        public DateTime Ngay { get; set; }
        public string GioBatDau { get; set; }
        public string GioKetThuc { get; set; }

        public string Phong { get; set; }
        public string DiaDiem { get; set; }

        public string LoaiBuoiHoc { get; set; }
        public string GhiChu { get; set; }
        public string TrangThai { get; set; }
        public int SoBuoi { get; set; }
    }
    public class LichSuKienDto
    {
        public int LichSuKienId { get; set; }
        public string TieuDe { get; set; }
        public string Loai { get; set; }

        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }

        public string LopHoc { get; set; }
        public string BuoiHoc { get; set; }
        public string MoTa { get; set; }
    }
    public class LichThiDto
    {
        public int BuoiThiId { get; set; }
        public string MonHoc { get; set; }       // Tên môn học
        public string Lop { get; set; }          // Mã lớp (MaLop)
        public string NgayThi { get; set; }        // Thứ/ngày/tháng/năm
        public string GioBatDau { get; set; }    // HH:mm
        public string GioKetThuc { get; set; }   // HH:mm
        public string GiangVien { get; set; }    // Tên giảng viên
        public string TenBaiThi { get; set; }    // Loại/ tên bài thi
        public string Loai { get; set; }         // online/offline
        public int TongSinhVien { get; set; }    // Số sinh viên tham gia
        public string DiaDiem { get; set; }      // Cơ sở – Tòa nhà – Phòng
    }
}
