using Humanizer;
using LMS_GV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Net.NetworkInformation;
using LMS_GV.Models.Data;
using LMS_GV.Models.DTO_GiangVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers_GiangVien
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Giảng Viên")]
    public class GV_SinhVienController : Controller
    {
        private readonly AppDbContext _context;

        public GV_SinhVienController(AppDbContext context)
        {
            _context = context;
        }

        // ----<TỔNG HỢP API QUẢN LÝ SINH VIÊN(SUMMARY)>----
        //1. API Trang Danh Sách Sinh Viên

        //GET /api/sinh-vien/thong-ke: Lấy thống kê sinh viên trong các lớp giảng viên phụ trách.

        //GET /api/sinh-vien: Lấy danh sách sinh viên, hỗ trợ tìm kiếm và bộ lọc theo khóa, ngành, khoa, lớp, trạng thái.

        //2. API Trang Chi Tiết Sinh Viên

        //GET /api/sinh-vien/{ id}/thong-tin: Lấy thông tin chi tiết hồ sơ của sinh viên.

        //GET /api/sinh-vien/{ id}/bang-diem: Lấy bảng điểm chi tiết theo từng môn của sinh viên.

        //3. API Sửa Điểm Sinh Viên

        //GET /api/sinh-vien/{id}/mon/{monId}/ diem: Lấy điểm chi tiết của sinh viên theo môn trước khi chỉnh sửa.

        //PUT /api/sinh-vien/{id}/ mon /{ monId}/ diem: Cập nhật điểm môn học; hệ thống tự tính lại điểm trung bình, điểm chữ và GPA.

        //----<TỔNG HỢP API QUẢN LÝ SINH VIÊN(SUMMARY)>----


        /// <summary>
        /// Lấy thống kê sinh viên trong tất cả lớp do giảng viên phụ trách:
        /// - Tổng số sinh viên
        /// - Số sinh viên đang hoạt động
        /// </summary>
        /// <summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("sinh-vien/all")]
        public async Task<ActionResult<StudentSummaryDto>> GetAllStudents()
        {
            // Lấy GiảngViênId từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Query lấy tất cả sinh viên mà giảng viên dạy
            var svQuery = from svl in _context.SinhVienLops
                          join sv in _context.HoSoSinhViens on svl.SinhVienId equals sv.SinhVienId
                          join nd in _context.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                          join ng in _context.Nganhs on sv.NganhId equals ng.NganhId
                          join kh in _context.Khoas on ng.KhoaId equals kh.KhoaId
                          where svl.LopHoc.GiangVienId == giangVienId
                          select new StudentListItemDto
                          {
                              SinhVienId = sv.SinhVienId,
                              MSSV = sv.Mssv,
                              HoTen = nd.HoTen,
                              Khoa = kh.TenKhoa,
                              Nganh = ng.TenNganh,
                              Lop = svl.LopHoc.TenLop,
                              TrangThai = svl.TinhTrang
                          };

            var list = await svQuery.ToListAsync();

            var result = new StudentSummaryDto
            {
                TongSinhVien = list.Count(),
                DangHoatDong = list.Count(x => x.TrangThai == "Hoạt động"),
                DungHoatDong = list.Count(x => x.TrangThai == "Dừng"),
                SinhViens = list
            };

            return Ok(result);

        }




        /// <summary>
        /// Lấy danh sách sinh viên trong các lớp giảng viên đang dạy.
        /// Hỗ trợ tìm kiếm theo Tên, MSSV.
        /// Hỗ trợ bộ lọc: khóa, ngành, khoa, lớp, trạng thái.
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("sinh-vien/filter")]
        public async Task<ActionResult<List<StudentListItemDto>>> FilterStudents([FromQuery] StudentFilterDto filter)
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // ===== Join thủ công các bảng để lấy thông tin Sinh viên + Nganh + Khoa =====
            var query = from svl in _context.SinhVienLops
                        join sv in _context.HoSoSinhViens on svl.SinhVienId equals sv.SinhVienId
                        join nd in _context.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                        join ng in _context.Nganhs on sv.NganhId equals ng.NganhId
                        join kh in _context.Khoas on ng.KhoaId equals kh.KhoaId
                        join lh in _context.LopHocs on svl.LopHocId equals lh.LopHocId
                        where lh.GiangVienId == giangVienId
                        select new { svl, sv, nd, ng, kh, lh };

            // ====== Tìm kiếm theo tên + MSSV ======
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var keyword = filter.Search.Trim();
                query = query.Where(x =>
                    x.nd.HoTen.Contains(keyword) ||
                    x.sv.NguoiDungId.ToString().Contains(keyword)
                );
            }

            // ====== Bộ lọc ======
            if (filter.KhoaId.HasValue)
                query = query.Where(x => x.kh.KhoaId == filter.KhoaId);

            if (filter.NganhId.HasValue)
                query = query.Where(x => x.ng.NganhId == filter.NganhId);

            if (filter.KhoaHocId.HasValue)
                query = query.Where(x => x.sv.KhoaTuyenSinhId == filter.KhoaHocId); // dùng KhoaTuyenSinhId nếu phù hợp

            if (filter.LopHocId.HasValue)
                query = query.Where(x => x.lh.LopHocId == filter.LopHocId);

            if (!string.IsNullOrWhiteSpace(filter.TrangThai))
                query = query.Where(x => x.svl.TinhTrang == filter.TrangThai);

            // ====== Mapping DTO ======
            var result = await query.Select(x => new StudentListItemDto
            {
                SinhVienId = x.sv.SinhVienId,
                MSSV = x.sv.NguoiDungId.ToString(),
                HoTen = x.nd.HoTen,
                Khoa = x.kh.TenKhoa,
                Nganh = x.ng.TenNganh,
                Lop = x.lh.TenLop,
                TrangThai = x.svl.TinhTrang
            }).ToListAsync();

            return Ok(result);
        }


        /// <summary>
        /// Xem thông tin chi tiết sinh viên:
        /// MSSV, hình ảnh, số điện thoại, email, họ tên, ngày sinh,
        /// tổng tín chỉ tích lũy, tổng GPA, khóa, ngành, trạng thái.
        /// Lấy bảng điểm chi tiết của sinh viên:
        /// - Tên môn học
        /// - Điểm bài tập, giữa kỳ, cuối kỳ, chuyên cần
        /// - Điểm trung bình môn, điểm chữ, GPA môn
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("sinh-vien/{sinhVienId}")]
        public async Task<ActionResult<StudentDetailDto>> GetStudentDetail(int sinhVienId)
        {
            // 1. Lấy thông tin sinh viên kèm NguoiDung và DangKyTinChi -> MonHoc -> LopHoc -> Nganh -> Khoa
            var student = await _context.HoSoSinhViens
               .Include(sv => sv.NguoiDung)
               .Include(sv => sv.DangKyTinChis)
                   .ThenInclude(dk => dk.MonHoc)
                       .ThenInclude(mh => mh.LopHocs)
                           .ThenInclude(lh => lh.Nganh)
                               .ThenInclude(n => n.Khoa)
               .FirstOrDefaultAsync(sv => sv.SinhVienId == sinhVienId);


            if (student == null)
                return NotFound("Không tìm thấy sinh viên");

            // 2. Lấy điểm thành phần
            var diemThanhPhans = await _context.DiemThanhPhans
                .Include(dtp => dtp.ThanhPhanDiem)  // Lấy tên thành phần và hệ số
                .Include(dtp => dtp.LopHoc)         // Lấy tên lớp -> MonHoc
                .Where(dtp => dtp.SinhVienId == sinhVienId)
                .ToListAsync();

            // 3. Lấy điểm chuyên cần
            var diemCCList = await _context.DiemChuyenCans
                .Where(dcc => dcc.SinhVienId == sinhVienId)
                .ToListAsync();

            // Hàm tính điểm chuyên cần theo số buổi vắng
            decimal TinhDiemCC(int soBuoiVang)
            {
                if (soBuoiVang == 0) return 10;
                if (soBuoiVang <= 2) return 9;
                return 0;
            }

            // 4. Tạo danh sách BangDiem
            var bangDiem = new List<MonHocBangDiemDto>();

            foreach (var dtpGroup in diemThanhPhans.GroupBy(d => d.LopHocId))
            {
                var lopHoc = dtpGroup.First().LopHoc;

                // Lấy điểm chuyên cần tương ứng lớp
                var diemCC = diemCCList.FirstOrDefault(d => d.LopHocId == lopHoc.LopHocId)?.Diem ?? 10;

                decimal diemBaiTap = 0, diemGiuaKy = 0, diemCuoiKy = 0;

                foreach (var dtp in dtpGroup)
                {
                    var tpName = dtp.ThanhPhanDiem.Ten.ToLower();
                    if (tpName.Contains("bài tập") || tpName.Contains("bài kiểm tra")) diemBaiTap = dtp.Diem ?? 0;
                    else if (tpName.Contains("giữa kỳ")) diemGiuaKy = dtp.Diem ?? 0;
                    else if (tpName.Contains("cuối kỳ")) diemCuoiKy = dtp.Diem ?? 0;
                }

                // Tính điểm trung bình môn theo trọng số
                decimal dtbMon = (diemCC * 10 + diemBaiTap * 20 + diemGiuaKy * 30 + diemCuoiKy * 40) / 100;

                // Quy đổi sang GPA
                float gpaMon;
                if (dtbMon >= 8.5m) gpaMon = 4.0f;
                else if (dtbMon >= 7) gpaMon = 3.5f;
                else if (dtbMon >= 6) gpaMon = 3.0f;
                else if (dtbMon >= 5) gpaMon = 2.5f;
                else if (dtbMon >= 4) gpaMon = 2.0f;
                else gpaMon = 0;

                bangDiem.Add(new MonHocBangDiemDto
                {
                    TenMon = lopHoc.MonHoc?.TenMon ?? lopHoc.TenLop,
                    DiemBaiTap = (float)diemBaiTap,
                    DiemGiuaKy = (float)diemGiuaKy,
                    DiemCuoiKy = (float)diemCuoiKy,
                    ChuyenCan = (float)diemCC,
                    TrungBinhMon = (float)dtbMon,
                    GPA_Mon = gpaMon,
                    DiemChu = "", // có thể tra bảng ThangDiem nếu có
                    SoTinChi = lopHoc.SoTinChi ?? 0
                });
            }

            // 5. Tính tổng tín chỉ và tổng GPA (trọng số) 
            float tongTC = bangDiem.Sum(b => (float)(b.SoTinChi ?? 0));
            float tongGPA = 0;

            if (bangDiem.Count > 0)
            {
                float numerator = bangDiem.Sum(b => b.GPA_Mon * (b.SoTinChi ?? 0));
                float denominator = tongTC > 0 ? tongTC : 1;
                tongGPA = numerator / denominator;
            }
            // Lấy DangKyTinChi đầu tiên
            var firstDangKy = student.DangKyTinChis.FirstOrDefault();
            var firstLopHoc = firstDangKy?.MonHoc?.LopHocs.FirstOrDefault();

            // Mapping DTO
            var dto = new StudentDetailDto
            {
                MSSV = student.Mssv, // lấy trực tiếp từ HoSoSinhVien
                HoTen = student.NguoiDung.HoTen,
                Email = student.NguoiDung.Email,
                SoDienThoai = student.NguoiDung.SoDienThoai,
                HinhAnh = student.NguoiDung.Avatar,
                NgaySinh = student.NguoiDung.NgaySinh.HasValue
         ? student.NguoiDung.NgaySinh.Value.ToDateTime(TimeOnly.MinValue)
         : default,
                Khoa = firstLopHoc?.Nganh?.Khoa?.TenKhoa ?? "",
                Nganh = firstLopHoc?.Nganh?.TenNganh ?? "",
                TrangThai = firstDangKy?.TrangThai.ToString() ?? "Chưa cập nhật",
                TongTC = tongTC,
                GPA = tongGPA,
                BangDiem = bangDiem
            };

            return Ok(dto);
        }

        /// <summary>
        /// Lấy điểm chi tiết của sinh viên theo môn:
        /// - Điểm bài tập, giữa kỳ, cuối kỳ, chuyên cần
        /// - Điểm trung bình môn
        /// - Điểm chữ và GPA môn
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("diem-mon/{diemId}")]
        public async Task<ActionResult<MonHocBangDiemDto>> GetDiemMon(int diemId)
        {
            // Lấy thông tin điểm + lớp + môn + sinh viên + người dùng
            var d = await (from diem in _context.Diems
                           join lop in _context.LopHocs on diem.LopHocId equals lop.LopHocId
                           join mh in _context.MonHocs on lop.MonHocId equals mh.MonHocId
                           join sv in _context.HoSoSinhViens on diem.SinhVienId equals sv.SinhVienId
                           join nd in _context.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                           where diem.DiemId == diemId
                           select new
                           {
                               Diem = diem,
                               MonHoc = mh,
                               LopHoc = lop,
                               SinhVien = sv,
                               NguoiDung = nd
                           }).FirstOrDefaultAsync();

            if (d == null) return NotFound();

            // Lấy tất cả điểm thành phần cho lớp này
            var diemThanhPhans = await _context.DiemThanhPhans
                .Include(dt => dt.ThanhPhanDiem)
                .Where(dt => dt.SinhVienId == d.SinhVien.SinhVienId && dt.LopHocId == d.LopHoc.LopHocId)
                .ToListAsync();

            // Lấy điểm chuyên cần
            var diemCC = await _context.DiemChuyenCans
                .Where(dc => dc.SinhVienId == d.SinhVien.SinhVienId && dc.LopHocId == d.LopHoc.LopHocId)
                .Select(dc => dc.Diem)
                .FirstOrDefaultAsync();

            decimal diemBaiTap = 0, diemGiuaKy = 0, diemCuoiKy = 0;
            foreach (var dtp in diemThanhPhans)
            {
                var tenTP = dtp.ThanhPhanDiem.Ten.ToLower();
                if (tenTP.Contains("bài tập") || tenTP.Contains("bài kiểm tra")) diemBaiTap = dtp.Diem ?? 0;
                else if (tenTP.Contains("giữa kỳ")) diemGiuaKy = dtp.Diem ?? 0;
                else if (tenTP.Contains("cuối kỳ")) diemCuoiKy = dtp.Diem ?? 0;
            }

            decimal dtbMon = ((diemCC ?? 10) * 10 + diemBaiTap * 20 + diemGiuaKy * 30 + diemCuoiKy * 40) / 100;

            // Mapping DTO
            var dto = new MonHocBangDiemDto
            {
                TenMon = d.MonHoc.TenMon,
                DiemBaiTap = (float?)diemBaiTap,
                DiemGiuaKy = (float?)diemGiuaKy,
                DiemCuoiKy = (float?)diemCuoiKy,
                ChuyenCan = (float?)diemCC,
                TrungBinhMon = (float)dtbMon,
                DiemChu = "", // có thể tra bảng ThangDiem nếu muốn
                GPA_Mon = 0,  // hoặc tính từ dtbMon
                SoTinChi = d.MonHoc.SoTinChi
            };

            return Ok(dto);
        }

        /// <summary>
        /// Cập nhật điểm sinh viên theo từng môn:
        /// - Giảng viên nhập: bài tập, giữa kỳ, cuối kỳ, chuyên cần
        /// - Hệ thống tự tính lại: điểm trung bình, điểm chữ, GPA môn
        /// </summary>

        [Authorize(Roles = "Giảng Viên")]
        [HttpPatch("diem-mon/{diemId}")]
        public async Task<IActionResult> UpdateDiemMon(int diemId, [FromBody] PatchStudentScoreDto dto)
        {
            // 1. Lấy điểm tổng môn
            var diem = await _context.Diems
                .Include(d => d.LopHoc)
                .Include(d => d.SinhVien)
                .FirstOrDefaultAsync(d => d.DiemId == diemId);

            if (diem == null)
                return NotFound("Không tìm thấy điểm môn");

            // 2. Cập nhật điểm chuyên cần
            var diemCC = await _context.DiemChuyenCans
                .FirstOrDefaultAsync(d => d.SinhVienId == dto.SinhVienId && d.LopHocId == dto.LopHocId);

            if (diemCC != null && dto.DiemChuyenCan.HasValue)
                diemCC.Diem = dto.DiemChuyenCan.Value;

            // 3. Cập nhật điểm thành phần (bài tập, giữa kỳ, cuối kỳ)
            var diemThanhPhans = await _context.DiemThanhPhans
                .Where(d => d.SinhVienId == dto.SinhVienId && d.LopHocId == dto.LopHocId)
                .ToListAsync();

            foreach (var dtp in diemThanhPhans)
            {
                // Giả sử ThanhPhanDiemId 1=bài tập, 2=giữa kỳ, 3=cuối kỳ
                if (dtp.ThanhPhanDiemId == 1 && dto.DiemBaiTap.HasValue) dtp.Diem = dto.DiemBaiTap.Value;
                if (dtp.ThanhPhanDiemId == 2 && dto.DiemGiuaKy.HasValue) dtp.Diem = dto.DiemGiuaKy.Value;
                if (dtp.ThanhPhanDiemId == 3 && dto.DiemCuoiKy.HasValue) dtp.Diem = dto.DiemCuoiKy.Value;
            }

            // 4. Tính điểm trung bình môn
            decimal diemCCVal = diemCC?.Diem ?? 0;
            decimal diemBT = diemThanhPhans.FirstOrDefault(d => d.ThanhPhanDiemId == 1)?.Diem ?? 0;
            decimal diemGK = diemThanhPhans.FirstOrDefault(d => d.ThanhPhanDiemId == 2)?.Diem ?? 0;
            decimal diemCK = diemThanhPhans.FirstOrDefault(d => d.ThanhPhanDiemId == 3)?.Diem ?? 0;

            diem.DiemTrungBinhMon = (diemCCVal * 0.1m + diemBT * 0.2m + diemGK * 0.3m + diemCK * 0.4m);

            // 5. Chuyển đổi sang điểm chữ và GPA
            diem.DiemChu = ConvertToLetter(diem.DiemTrungBinhMon.Value);
            diem.Gpamon = ConvertToGPA(diem.DiemChu);

            await _context.SaveChangesAsync();
            return Ok("Cập nhật điểm thành công");
        }

        private string ConvertToLetter(decimal dtbMon)
        {
            var thangDiem = _context.ThangDiems
                .FirstOrDefault(td => dtbMon >= td.DiemMin && dtbMon <= td.DiemMax);

            return thangDiem?.DiemChu ?? "F";
        }

        //// Chuyển điểm chữ sang GPA
        private decimal ConvertToGPA(string diemChu)
        {
            return diemChu switch
            {
                "A" => 4.0m,
                "B+" => 3.5m,
                "B" => 3.0m,
                "C+" => 2.5m,
                "C" => 2.0m,
                _ => 0m
            };
        }

    }
}
