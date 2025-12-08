namespace LMS_GV.Models.DTO_GiangVien
{
    /// <summary>
    /// DTO: Tổng quan Dashboard giảng viên
    /// </summary>
    public class InstructorDashboardDto
    {
        public List<TeachingScheduleDto> TodaySchedule { get; set; }   // Lịch dạy hôm nay
        public int TotalActiveClasses { get; set; }                    // Tổng số lớp đang dạy
        public int TotalManagedStudents { get; set; }                  // Tổng số sinh viên đang quản lý
        public List<HomeworkChartDto> HomeworkChart { get; set; }      // Biểu đồ hoàn thành bài tập
    }

    /// <summary>
    /// DTO: Lịch dạy hôm nay
    /// </summary>
    public class TeachingScheduleDto
    {
        public string BuoiHocId { get; set; }
        public string TenMon { get; set; }
        public string PhongHoc { get; set; }      // Phòng học (nếu trạng thái Bình thường)
        public string TrangThai { get; set; } // Trạng thái buỏi học ( 0=Đã hủy, 1=Bình thường, 2=Đổi giờ, 3=Đổi phòng, 4=Online)
        public DateTime GioBatDau { get; set; }
        public DateTime GioKetThuc { get; set; }
        public int SoLuongSinhVien { get; set; }
    }


    /// <summary>
    /// DTO: Biểu đồ hoàn thành bài tập
    /// </summary>
    /// // Bảng bài nộp (BaiNop)
    public class HomeworkChartDto
    {
        public string TenMon { get; set; }   // Tên môn
        public byte? TrangThai { get; set; }    //  0 = gian lận, 1 = đã chấm , 2 = đã nộp, 3 = chưa nộp
    }

    public class RecentActivityDto
    {
        public int NhatKyHeThongId { get; set; }
        public string? NoiDung { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
