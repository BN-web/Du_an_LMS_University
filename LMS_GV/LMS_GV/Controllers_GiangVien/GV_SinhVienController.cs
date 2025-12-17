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
using System.ComponentModel.DataAnnotations;

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
        /// <summary>
        /// Xem chi tiết sinh viên + bảng điểm (CHỈ ĐỌC)
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("sinh-vien/{sinhVienId}")]
        public async Task<ActionResult<StudentDetailDto>> GetStudentDetail(int sinhVienId)
        {
            // 1. Thông tin sinh viên
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

            // 2. BẢNG ĐIỂM TỔNG (nguồn chuẩn)
            var diemTongList = await _context.Diems
                .Where(d => d.SinhVienId == sinhVienId)
                .ToListAsync();

            // 3. ĐIỂM THÀNH PHẦN
            var diemThanhPhans = await _context.DiemThanhPhans
                .Include(dtp => dtp.ThanhPhanDiem)
                .Include(dtp => dtp.LopHoc)
                .Where(dtp => dtp.SinhVienId == sinhVienId)
                .ToListAsync();

            // 4. ĐIỂM CHUYÊN CẦN
            var diemCCList = await _context.DiemChuyenCans
                .Where(d => d.SinhVienId == sinhVienId)
                .ToListAsync();

            var bangDiem = new List<MonHocBangDiemDto>();

            foreach (var lopGroup in diemThanhPhans.GroupBy(x => x.LopHocId))
            {
                var lopHoc = lopGroup.First().LopHoc;

                var diemTong = diemTongList
                    .FirstOrDefault(d => d.LopHocId == lopHoc.LopHocId);

                var diemCC = diemCCList
                    .FirstOrDefault(d => d.LopHocId == lopHoc.LopHocId)?.Diem;

                // Map điểm thành phần theo tên CHUẨN
                float? diemBaiTap = null;
                float? diemGiuaKy = null;
                float? diemCuoiKy = null;

                foreach (var tp in lopGroup)
                {
                    switch (tp.ThanhPhanDiem.Ten)
                    {
                        case "Điểm bài tập":
                            diemBaiTap = (float?)tp.Diem;
                            break;

                        case "Điểm giữa kỳ":
                        case "Điểm kiểm tra":
                            diemGiuaKy = (float?)tp.Diem;
                            break;

                        case "Điểm cuối kỳ":
                            diemCuoiKy = (float?)tp.Diem;
                            break;
                    }
                }

                bangDiem.Add(new MonHocBangDiemDto
                {
                    DiemId = diemTong?.DiemId,
                    LopHocId = lopHoc.LopHocId,

                    TenMon = lopHoc.MonHoc?.TenMon ?? lopHoc.TenLop,

                    // 👉 CHỈ HIỂN THỊ
                    DiemBaiTap = diemBaiTap,
                    DiemGiuaKy = diemGiuaKy,
                    DiemCuoiKy = diemCuoiKy,
                    ChuyenCan = (float?)diemCC,

                    // 👉 ĐỌC TỪ BẢNG DIEMS (PATCH ĐÃ TÍNH)
                    TrungBinhMon = (float)(diemTong?.DiemTrungBinhMon ?? 0),
                    DiemChu = diemTong?.DiemChu,
                    GPA_Mon = (float)(diemTong?.Gpamon ?? 0),
                    SoTinChi = diemTong?.SoTinChi ?? lopHoc.SoTinChi
                });
            }

            // 5. TÍNH GPA TOÀN KHÓA
            float tongTC = bangDiem.Sum(x => (float)(x.SoTinChi ?? 0));
            float tongGPA = tongTC > 0
                ? bangDiem.Sum(x => x.GPA_Mon * (x.SoTinChi ?? 0)) / tongTC
                : 0;

            var firstDangKy = student.DangKyTinChis.FirstOrDefault();
            var firstLopHoc = firstDangKy?.MonHoc?.LopHocs.FirstOrDefault();

            return Ok(new StudentDetailDto
            {
                MSSV = student.Mssv,
                HoTen = student.NguoiDung.HoTen,
                Email = student.NguoiDung.Email,
                SoDienThoai = student.NguoiDung.SoDienThoai,
                HinhAnh = student.NguoiDung.Avatar,
                NgaySinh = student.NguoiDung.NgaySinh?.ToDateTime(TimeOnly.MinValue) ?? default,
                Khoa = firstLopHoc?.Nganh?.Khoa?.TenKhoa ?? "",
                Nganh = firstLopHoc?.Nganh?.TenNganh ?? "",
                TrangThai = firstDangKy?.TrangThai.ToString() ?? "",
                TongTC = tongTC,
                GPA = tongGPA,
                BangDiem = bangDiem
            });
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
            var diem = await _context.Diems
                .Include(d => d.LopHoc)
                    .ThenInclude(lh => lh.MonHoc)
                .FirstOrDefaultAsync(d => d.DiemId == diemId);

            if (diem == null)
                return NotFound("Không tìm thấy điểm");

            var diemThanhPhans = await _context.DiemThanhPhans
                .Include(d => d.ThanhPhanDiem)
                .Where(d =>
                    d.SinhVienId == diem.SinhVienId &&
                    d.LopHocId == diem.LopHocId)
                .ToListAsync();

            var diemCC = await _context.DiemChuyenCans
                .Where(d =>
                    d.SinhVienId == diem.SinhVienId &&
                    d.LopHocId == diem.LopHocId)
                .Select(d => d.Diem)
                .FirstOrDefaultAsync();

            decimal? diemBT = null, diemGK = null, diemCK = null;

            foreach (var tp in diemThanhPhans)
            {
                var ten = tp.ThanhPhanDiem.Ten.ToLower();
                if (ten.Contains("bài")) diemBT = tp.Diem;
                else if (ten.Contains("giữa")) diemGK = tp.Diem;
                else if (ten.Contains("cuối")) diemCK = tp.Diem;
            }

            return Ok(new MonHocBangDiemDto
            {
                DiemId = diem.DiemId,
                LopHocId = diem.LopHocId,
                TenMon = diem.LopHoc.MonHoc?.TenMon,

                DiemBaiTap = (float?)diemBT,
                DiemGiuaKy = (float?)diemGK,
                DiemCuoiKy = (float?)diemCK,
                ChuyenCan = (float?)diemCC,

                // 👉 CHỈ ĐỌC DB
                TrungBinhMon = (float)(diem.DiemTrungBinhMon ?? 0),
                DiemChu = diem.DiemChu,
                GPA_Mon = (float)(diem.Gpamon ?? 0),
                SoTinChi = diem.SoTinChi
            });
        }


        /// <summary>
        /// Cập nhật điểm sinh viên theo từng môn:
        /// - Giảng viên nhập: bài tập, giữa kỳ, cuối kỳ, chuyên cần
        /// - Hệ thống tự tính lại: điểm trung bình, điểm chữ, GPA môn
        /// </summary>      
        /// <summary>
        /// Cập nhật điểm môn học của sinh viên
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpPatch("diem-mon/{diemId}")]
        public async Task<IActionResult> UpdateDiemMon(
            int diemId,
            [FromBody] PatchStudentScoreDto dto)
        {
            // =======================
            // 0. Validate input
            // =======================
            var error = ValidateScores(dto);
            if (error != null) return BadRequest(error);

            // =======================
            // 1. Lấy GiảngViênId từ token
            // =======================
            var giangVienClaim = User.FindFirst("GiangVien_id");
            if (giangVienClaim == null)
                return Unauthorized("Token không chứa GiangVien_id");

            int giangVienId = int.Parse(giangVienClaim.Value);

            // =======================
            // 2. Lấy bảng điểm + check quyền
            // =======================
            var diem = await _context.Diems
                .Include(d => d.LopHoc)
                .FirstOrDefaultAsync(d =>
                    d.DiemId == diemId &&
                    d.LopHoc.GiangVienId == giangVienId);

            if (diem == null)
                return NotFound("Không tìm thấy điểm hoặc không có quyền chỉnh sửa");

            // =======================
            // 3. Lấy cấu hình thành phần điểm của lớp
            // =======================
            var thanhPhanConfigs = await _context.ThanhPhanDiems
                .Where(tp => tp.LopHocId == diem.LopHocId)
                .ToListAsync();

            if (!thanhPhanConfigs.Any())
                return BadRequest("Lớp học chưa cấu hình thành phần điểm");

            // =======================
            // 4. Lấy / tạo điểm chuyên cần
            // =======================
            var diemChuyenCan = await _context.DiemChuyenCans
                .FirstOrDefaultAsync(d =>
                    d.SinhVienId == diem.SinhVienId &&
                    d.LopHocId == diem.LopHocId);

            if (diemChuyenCan == null)
            {
                diemChuyenCan = new DiemChuyenCan
                {
                    SinhVienId = diem.SinhVienId,
                    LopHocId = diem.LopHocId,
                    HocKyId = diem.HocKyId,
                    Diem = dto.DiemChuyenCan ?? 10,
                    CreatedAt = DateTime.Now
                };
                _context.DiemChuyenCans.Add(diemChuyenCan);
            }
            else if (dto.DiemChuyenCan.HasValue)
            {
                diemChuyenCan.Diem = dto.DiemChuyenCan.Value;
                diemChuyenCan.UpdatedAt = DateTime.Now;
            }

            // =======================
            // 5. Lấy điểm thành phần (BÀI TẬP – GIỮA – CUỐI)
            // =======================
            var diemThanhPhans = await _context.DiemThanhPhans
                .Include(tp => tp.ThanhPhanDiem)
                .Where(tp =>
                    tp.SinhVienId == diem.SinhVienId &&
                    tp.LopHocId == diem.LopHocId)
                .ToListAsync();

            // =======================
            // 6. Update điểm thành phần
            // =======================
            foreach (var tp in diemThanhPhans)
            {
                var ten = tp.ThanhPhanDiem.Ten;

                if ((ten == "Điểm giữa kỳ" || ten == "Điểm kiểm tra")
                    && dto.DiemGiuaKy.HasValue)
                {
                    tp.Diem = dto.DiemGiuaKy.Value;
                    tp.UpdatedAt = DateTime.Now;
                }

                if (ten == "Điểm cuối kỳ" && dto.DiemCuoiKy.HasValue)
                {
                    tp.Diem = dto.DiemCuoiKy.Value;
                    tp.UpdatedAt = DateTime.Now;
                }
            }

            // =======================
            // 7. TÍNH ĐIỂM TRUNG BÌNH MÔN (NGUỒN CHUẨN)
            // =======================
            decimal tongDiem = 0m;
            decimal tongHeSo = 0m;

            // Thành phần điểm (trừ chuyên cần)
            foreach (var tp in diemThanhPhans)
            {
                var heSo = tp.ThanhPhanDiem.HeSo ?? 0m;
                tongDiem += (tp.Diem ?? 0m) * heSo;
                tongHeSo += heSo;
            }

            // Chuyên cần (lấy hệ số từ cấu hình)
            var tpChuyenCan = thanhPhanConfigs
                .FirstOrDefault(tp => tp.Ten.Contains("chuyên"));

            if (tpChuyenCan != null)
            {
                tongDiem += (diemChuyenCan.Diem ?? 0m) * (tpChuyenCan.HeSo ?? 0m);
                tongHeSo += tpChuyenCan.HeSo ?? 0m;
            }

            if (tongHeSo == 0)
                return BadRequest("Tổng hệ số = 0, không thể tính điểm");

            decimal diemTB = Math.Round(tongDiem / tongHeSo, 2);
            if (diemTB > 10) diemTB = 10;

            // =======================
            // 8. Cập nhật bảng DIEM (NGUỒN DUY NHẤT)
            // =======================
            var diemCu = diem.DiemTrungBinhMon;

            diem.DiemTrungBinhMon = diemTB;
            diem.DiemChu = ConvertToLetter(diemTB);
            diem.Gpamon = ConvertToGPA(diem.DiemChu);
            diem.TrangThai = diemTB >= 4 ? (byte)1 : (byte)0;
            diem.UpdatedAt = DateTime.Now;

            // =======================
            // 9. GHI LỊCH SỬ SỬA ĐIỂM (AN TOÀN)
            // =======================
            var nguoiDungClaim = User.FindFirst("NguoiDung_id");
            int? nguoiDungId = nguoiDungClaim != null
                ? int.Parse(nguoiDungClaim.Value)
                : null;

            _context.LichSuSuaDiems.Add(new LichSuSuaDiem
            {
                DiemId = diem.DiemId,
                NguoiDungId = nguoiDungId,
                DiemLucDau = diemCu,
                DiemMoi = diemTB,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật điểm môn thành công",
                diemTrungBinh = diemTB,
                diemChu = diem.DiemChu,
                gpa = diem.Gpamon
            });
        }


        // Validate điểm chỉ cho các điểm được phép
        private string? ValidateScores(PatchStudentScoreDto dto)
        {
            bool IsValid(decimal? v)
            {
                if (!v.HasValue) return true;
                return v.Value >= 0 && v.Value <= 10;
            }

            if (!IsValid(dto.DiemGiuaKy))
                return "Điểm giữa kỳ phải nằm trong khoảng 0 - 10";

            if (!IsValid(dto.DiemCuoiKy))
                return "Điểm cuối kỳ phải nằm trong khoảng 0 - 10";

            if (dto.DiemChuyenCan.HasValue && !new decimal[] { 10, 9, 0 }.Contains(dto.DiemChuyenCan.Value))
                return "Điểm chuyên cần chỉ được nhập: 10, 9 hoặc 0.";

            return null;
        }

        // Chuyển điểm trung bình môn sang chữ
        private string ConvertToLetter(decimal dtbMon)
        {
            var thangDiem = _context.ThangDiems
                .FirstOrDefault(td => dtbMon >= td.DiemMin && dtbMon <= td.DiemMax);

            return thangDiem?.DiemChu ?? "F";
        }

        // Chuyển điểm chữ sang GPA
        private decimal ConvertToGPA(string? diemChu)
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


        [HttpPost("tao-bang-diem")]
        public async Task<IActionResult> TaoBangDiem(TaoBangDiemRequestDTO req)
        {
            // 1. Lấy thành phần điểm của lớp
            var thanhPhansDB = await _context.ThanhPhanDiems
                .Where(tp => tp.LopHocId == req.LopHocId)
                .ToListAsync();

            if (!thanhPhansDB.Any()) return BadRequest("Lớp học chưa được cấu hình thành phần điểm");

            decimal tongDiem = 0m;
            foreach (var item in req.DiemThanhPhans)
            {
                var tp = thanhPhansDB.FirstOrDefault(x => x.Ten.Trim().ToLower() == item.TenThanhPhan.Trim().ToLower());
                if (tp == null) return BadRequest($"Thành phần điểm không tồn tại: {item.TenThanhPhan}");
                if (item.Diem < 0 || item.Diem > 10) return BadRequest($"Điểm {item.TenThanhPhan} phải từ 0 đến 10");

                _context.DiemThanhPhans.Add(new DiemThanhPhan
                {
                    SinhVienId = req.SinhVienId,
                    LopHocId = req.LopHocId,
                    ThanhPhanDiemId = tp.ThanhPhanDiemId,
                    Diem = item.Diem,
                    GhiChu = item.GhiChu,
                    CreatedAt = DateTime.Now
                });

                decimal heSo = tp.HeSo ?? 0m;
                tongDiem += item.Diem * heSo;
            }

            decimal diemTB = Math.Round(tongDiem, 2);
            var thangDiem = await _context.ThangDiems.FirstOrDefaultAsync(x => diemTB >= x.DiemMin && diemTB <= x.DiemMax);

            var diem = new Diem
            {
                SinhVienId = req.SinhVienId,
                LopHocId = req.LopHocId,
                HocKyId = req.HocKyId,
                DiemTrungBinhMon = diemTB,
                DiemChu = thangDiem?.DiemChu ?? "F",
                Gpamon = ConvertToGPA(thangDiem?.DiemChu),
                TrangThai = diemTB >= 4 ? (byte)1 : (byte)0,
                CreatedAt = DateTime.Now
            };

            _context.Diems.Add(diem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo bảng điểm thành công",
                diemTrungBinh = diemTB,
                diemChu = diem.DiemChu
            });
        }

        [Authorize(Roles = "Giảng Viên")]
        [HttpPost("lop-hoc/them-sinh-vien")]
        public async Task<IActionResult> AddStudentToClass(
[FromBody] AddStudentToClassDto dto)
        {
            // =======================
            // 1. Lấy giảng viên từ token
            // =======================
            var giangVienClaim = User.FindFirst("GiangVien_id");
            if (giangVienClaim == null)
                return Unauthorized("Token không chứa GiangVien_id");

            int giangVienId = int.Parse(giangVienClaim.Value);

            // =======================
            // 2. Kiểm tra lớp có thuộc giảng viên không
            // =======================
            var lopHoc = await _context.LopHocs
                .FirstOrDefaultAsync(lh =>
                    lh.LopHocId == dto.LopHocId &&
                    lh.GiangVienId == giangVienId);

            if (lopHoc == null)
                return BadRequest("Bạn không có quyền thao tác lớp này");

            // =======================
            // 3. Kiểm tra sinh viên tồn tại
            // =======================
            var sinhVien = await _context.HoSoSinhViens
                .FirstOrDefaultAsync(sv => sv.SinhVienId == dto.SinhVienId);

            if (sinhVien == null)
                return NotFound("Không tìm thấy sinh viên");

            // =======================
            // 4. Kiểm tra sinh viên đã vào lớp chưa
            // =======================
            bool daCo = await _context.SinhVienLops.AnyAsync(x =>
                x.SinhVienId == dto.SinhVienId &&
                x.LopHocId == dto.LopHocId);

            if (daCo)
                return BadRequest("Sinh viên đã tồn tại trong lớp");

            // =======================
            // 5. Ghi danh sinh viên vào lớp
            // =======================
            _context.SinhVienLops.Add(new SinhVienLop
            {
                SinhVienId = dto.SinhVienId,
                LopHocId = dto.LopHocId,
                TinhTrang = "Hoạt động",
                CreatedAt = DateTime.Now
            });

            // =======================
            // 6. Tạo bảng DIEM (NGUỒN CHUẨN)
            // =======================
            var diem = new Diem
            {
                SinhVienId = dto.SinhVienId,
                LopHocId = dto.LopHocId,
                HocKyId = dto.HocKyId,
                DiemTrungBinhMon = 0,
                DiemChu = "F",
                Gpamon = 0,
                HeSoMon = 1,
                SoTinChi = lopHoc.SoTinChi,
                TrangThai = 1,
                CreatedAt = DateTime.Now
            };

            _context.Diems.Add(diem);

            // =======================
            // 7. Tạo điểm CHUYÊN CẦN
            // =======================
            _context.DiemChuyenCans.Add(new DiemChuyenCan
            {
                SinhVienId = dto.SinhVienId,
                LopHocId = dto.LopHocId,
                HocKyId = dto.HocKyId,
                Diem = 10,
                CreatedAt = DateTime.Now
            });

            // =======================
            // 8. Tạo điểm THÀNH PHẦN (Bài tập – Giữa – Cuối)
            // =======================
            var thanhPhans = await _context.ThanhPhanDiems
                .Where(tp => tp.LopHocId == dto.LopHocId)
                .ToListAsync();

            if (!thanhPhans.Any())
                return BadRequest("Lớp chưa cấu hình thành phần điểm");

            foreach (var tp in thanhPhans)
            {
                // ❌ Không tạo chuyên cần ở DiemThanhPhan
                if (tp.Ten.Contains("chuyên", StringComparison.OrdinalIgnoreCase))
                    continue;

                _context.DiemThanhPhans.Add(new DiemThanhPhan
                {
                    SinhVienId = dto.SinhVienId,
                    LopHocId = dto.LopHocId,
                    ThanhPhanDiemId = tp.ThanhPhanDiemId,
                    Diem = null,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm sinh viên vào lớp và tạo bảng điểm thành công",
                sinhVienId = dto.SinhVienId,
                lopHocId = dto.LopHocId
            });
        }

        public class AddStudentToClassDto
        {
            [Required]
            public int SinhVienId { get; set; }

            [Required]
            public int LopHocId { get; set; }

            [Required]
            public int HocKyId { get; set; }

            // Optional
            public string? GhiChu { get; set; }
        }

    }
}
