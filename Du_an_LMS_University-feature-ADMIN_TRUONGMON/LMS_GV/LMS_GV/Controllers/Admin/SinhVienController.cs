using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using LMS_GV.Models;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/sinh-vien")]
    [Authorize(Roles = "Admin")]
    public class SinhVienController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SinhVienController(AppDbContext db)
        {
            _db = db;
        }

        // Helper: convert DateTime? -> DateOnly?
        private static DateOnly? ToDateOnly(DateTime? dt)
        {
            return dt.HasValue ? DateOnly.FromDateTime(dt.Value) : (DateOnly?)null;
        }

        private static string GenerateRandomPassword(int length = 10)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[bytes[i] % chars.Length]);
            }
            return sb.ToString();
        }

        // ======================= DTOs =======================

        public class SinhVienListQuery
        {
            public string? Search { get; set; }
            public int? NganhId { get; set; }
            public int? KhoaTuyenSinhId { get; set; }
            public byte? Status { get; set; } // 0=bị khóa,1=đang hoạt động,2=đã tốt nghiệp
        }

        public class CreateSinhVienRequest
        {
            [Required]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã sinh viên phải gồm đúng 6 ký tự số")]
            public string Mssv { get; set; } = string.Empty;

            [Required(ErrorMessage = "Họ tên là bắt buộc")]
            public string FullName { get; set; } = string.Empty;

            public string? Gender { get; set; }       // Nam/Nu/Khac
            public DateTime? Dob { get; set; }        // Ngày sinh
            public string? Phone { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            public int? NganhId { get; set; }
            public int? KhoaTuyenSinhId { get; set; }
            public string? Avatar { get; set; }
            public string? Address { get; set; }

            // Cho phép truyền vào, nhưng nếu null thì mặc định 1 = đang hoạt động
            public byte? Status { get; set; }
        }

        public class UpdateSinhVienRequest
        {
            [Required(ErrorMessage = "Họ tên là bắt buộc")]
            public string FullName { get; set; } = string.Empty;

            public string? Gender { get; set; }
            public DateTime? Dob { get; set; }
            public string? Phone { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            public int? NganhId { get; set; }
            public int? KhoaTuyenSinhId { get; set; }
            public string? Avatar { get; set; }
            public string? Address { get; set; }

            // Cho phép chỉnh sửa MSSV nếu cần
            [StringLength(6, MinimumLength = 6)]
            public string? Mssv { get; set; }

            public decimal? Gpa { get; set; }
            public int? TongTinChi { get; set; }
        }

        public class UpdateSinhVienStatusRequest
        {
            [Required]
            public byte Status { get; set; } // 0=bị khóa,1=đang hoạt động,2=đã tốt nghiệp
        }

        // ======================= Helper tính GPA =======================
        // Tính GPA từ bảng Diem:
        // GPA = Σ (GpaMon * SoTinChi) / Σ SoTinChi  với những môn TrangThai = 1 (đậu)
        private async Task<(int TotalCredits, decimal? Gpa)> ComputeGpaAsync(int sinhVienId)
        {
            var diemList = await _db.Diems
                .AsNoTracking()
                .Where(d => d.SinhVienId == sinhVienId && d.TrangThai == 1)
                .ToListAsync();

            if (!diemList.Any())
            {
                return (0, null);
            }

            var totalCredits = diemList.Sum(d => d.SoTinChi ?? 0);
            if (totalCredits <= 0)
            {
                return (0, null);
            }

            // Giả sử entity Diem có property GpaMon map tới cột GPAMon
            decimal sumGpa = diemList.Sum(d => (d.Gpamon ?? 0m) * (d.SoTinChi ?? 0));

            var gpa = sumGpa / totalCredits;

            return (totalCredits, Math.Round(gpa, 2));
        }

        // ======================= 1. GET / =======================
        // Danh sách + tìm kiếm + filter
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] SinhVienListQuery queryModel)
        {
            var query = _db.HoSoSinhViens
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var kw = queryModel.Search.Trim();
                query = query.Where(sv =>
                    (sv.Mssv ?? "").Contains(kw) ||
                    _db.NguoiDungs.Any(nd =>
                        nd.NguoiDungId == sv.NguoiDungId &&
                        (nd.HoTen ?? "").Contains(kw)));
            }

            if (queryModel.NganhId.HasValue)
            {
                query = query.Where(sv => sv.NganhId == queryModel.NganhId.Value);
            }

            if (queryModel.KhoaTuyenSinhId.HasValue)
            {
                query = query.Where(sv => sv.KhoaTuyenSinhId == queryModel.KhoaTuyenSinhId.Value);
            }

            if (queryModel.Status.HasValue)
            {
                var st = queryModel.Status.Value;
                query = query.Where(sv =>
                    _db.NguoiDungs.Any(nd =>
                        nd.NguoiDungId == sv.NguoiDungId &&
                        nd.TrangThai == st));
            }

            var list = await query
                .OrderByDescending(sv => sv.CreatedAt)
                .Select(sv => new
                {
                    id = sv.SinhVienId,
                    mssv = sv.Mssv,
                    fullName = _db.NguoiDungs
                        .Where(nd => nd.NguoiDungId == sv.NguoiDungId)
                        .Select(nd => nd.HoTen)
                        .FirstOrDefault(),
                    majorId = sv.NganhId,
                    majorName = _db.Nganhs
                        .Where(n => n.NganhId == sv.NganhId)
                        .Select(n => n.TenNganh)
                        .FirstOrDefault(),
                    cohortId = sv.KhoaTuyenSinhId,
                    cohortCode = _db.KhoaTuyenSinhs
                        .Where(k => k.KhoaTuyenSinhId == sv.KhoaTuyenSinhId)
                        .Select(k => k.MaKhoaTuyenSinh)
                        .FirstOrDefault(),
                    cohortName = _db.KhoaTuyenSinhs
                        .Where(k => k.KhoaTuyenSinhId == sv.KhoaTuyenSinhId)
                        .Select(k => k.TenKhoaTuyenSinh)
                        .FirstOrDefault(),
                    createdAt = sv.CreatedAt,
                    status = _db.NguoiDungs
                        .Where(nd => nd.NguoiDungId == sv.NguoiDungId)
                        .Select(nd => nd.TrangThai)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(list);
        }

        // ======================= 2. GET /{id} =======================
        // Chi tiết hồ sơ + GPA tính từ bảng Diem
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var sv = await _db.HoSoSinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SinhVienId == id);

            if (sv == null)
                return NotFound(new { message = "Không tìm thấy sinh viên" });

            var nd = await _db.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NguoiDungId == sv.NguoiDungId);

            var major = await _db.Nganhs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NganhId == sv.NganhId);

            var cohort = await _db.KhoaTuyenSinhs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.KhoaTuyenSinhId == sv.KhoaTuyenSinhId);

            var totalClasses = await _db.SinhVienLops
                .Where(x => x.SinhVienId == sv.SinhVienId)
                .Select(x => x.LopHocId)
                .Distinct()
                .CountAsync();

            var gpaData = await ComputeGpaAsync(sv.SinhVienId);

            var result = new
            {
                id = sv.SinhVienId,
                mssv = sv.Mssv,
                fullName = nd?.HoTen,
                gender = nd?.GioiTinh,
                dob = nd?.NgaySinh,
                phone = nd?.SoDienThoai,
                email = nd?.Email,
                avatar = nd?.Avatar,
                address = nd?.DiaChi,
                majorId = sv.NganhId,
                majorName = major?.TenNganh,
                cohortId = sv.KhoaTuyenSinhId,
                cohortCode = cohort?.MaKhoaTuyenSinh,
                cohortName = cohort?.TenKhoaTuyenSinh,
                // Ưu tiên GPA & tổng tín chỉ tính được từ bảng Diem
                gpa = gpaData.Gpa ?? sv.Gpa,
                totalCredits = gpaData.TotalCredits > 0 ? gpaData.TotalCredits : sv.TongTinChi,
                status = nd?.TrangThai,
                createdAt = sv.CreatedAt,
                updatedAt = sv.UpdatedAt,
                totalClasses
            };

            return Ok(result);
        }

        // ======================= API riêng tính GPA =======================
        // GET /api/admin/sinh-vien/{id}/gpa
        [HttpGet("{id:int}/gpa")]
        public async Task<IActionResult> GetGpa(int id)
        {
            var exists = await _db.HoSoSinhViens
                .AnyAsync(s => s.SinhVienId == id);
            if (!exists)
                return NotFound(new { message = "Không tìm thấy sinh viên" });

            var data = await ComputeGpaAsync(id);

            return Ok(new
            {
                sinhVienId = id,
                totalCredits = data.TotalCredits,
                gpa = data.Gpa
            });
        }

        // ======================= 3. POST / =======================
        // Thêm sinh viên
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSinhVienRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check MSSV trùng
            var existsMssv = await _db.HoSoSinhViens
                .AnyAsync(s => s.Mssv == req.Mssv);
            if (existsMssv)
            {
                return Conflict(new
                {
                    field = "mssv",
                    message = "Mã sinh viên đã tồn tại"
                });
            }

            if (string.IsNullOrWhiteSpace(req.Email))
            {
                return BadRequest(new
                {
                    field = "email",
                    message = "Email là bắt buộc"
                });
            }

            var existsEmail = await _db.NguoiDungs
                .AnyAsync(u => u.Email == req.Email || u.TenDangNhap == req.Email);
            if (existsEmail)
            {
                return Conflict(new
                {
                    field = "email",
                    message = "Email đã tồn tại"
                });
            }

            // Tạo tài khoản người dùng (VaiTro_id = 4: Sinh viên)
            var initialPassword = GenerateRandomPassword(10);
            var user = new NguoiDung
            {
                TenDangNhap = (req.Email ?? string.Empty).Trim(),
                HoTen = req.FullName,
                GioiTinh = req.Gender,
                NgaySinh = ToDateOnly(req.Dob),
                SoDienThoai = req.Phone,
                Email = req.Email,
                Avatar = req.Avatar,
                DiaChi = req.Address,
                TrangThai = req.Status ?? 1, // mặc định đang hoạt động
                VaiTroId = 4,
                CreatedAt = DateTime.UtcNow,
                HashMatKhau = BCrypt.Net.BCrypt.HashPassword(initialPassword)
            };
            _db.NguoiDungs.Add(user);
            await _db.SaveChangesAsync();

            var sv = new HoSoSinhVien
            {
                Mssv = req.Mssv,
                NguoiDungId = user.NguoiDungId,
                NganhId = req.NganhId,
                KhoaTuyenSinhId = req.KhoaTuyenSinhId,
                ThoiGianDaoTao = null, // có thể generate từ KhoaTuyenSinh nếu muốn
                TongTinChi = 0,
                Gpa = 0,
                CreatedAt = DateTime.UtcNow
            };
            _db.HoSoSinhViens.Add(sv);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetail), new { id = sv.SinhVienId }, new
            {
                id = sv.SinhVienId,
                mssv = sv.Mssv,
                username = user.TenDangNhap,
                initialPassword
            });
        }

        // ======================= 4. PUT /{id} =======================
        // Sửa sinh viên + auto tốt nghiệp nếu đủ 200 tín chỉ
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSinhVienRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sv = await _db.HoSoSinhViens
                .FirstOrDefaultAsync(s => s.SinhVienId == id);
            var user = sv != null
                ? await _db.NguoiDungs.FirstOrDefaultAsync(u => u.NguoiDungId == sv.NguoiDungId)
                : null;

            if (sv == null)
                return NotFound(new { message = "Không tìm thấy sinh viên" });

            // Nếu sửa MSSV, check trùng
            if (!string.IsNullOrWhiteSpace(req.Mssv) && req.Mssv != sv.Mssv)
            {
                var existsMssv = await _db.HoSoSinhViens
                    .AnyAsync(s => s.Mssv == req.Mssv && s.SinhVienId != id);
                if (existsMssv)
                {
                    return Conflict(new
                    {
                        field = "mssv",
                        message = "Mã sinh viên đã tồn tại"
                    });
                }
                sv.Mssv = req.Mssv;
                if (user != null)
                {
                    user.TenDangNhap = req.Mssv;
                }
            }

            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(req.Email) && req.Email != user.Email)
                {
                    var existsEmail = await _db.NguoiDungs
                        .AnyAsync(u => u.Email == req.Email && u.NguoiDungId != user.NguoiDungId);
                    if (existsEmail)
                    {
                        return Conflict(new
                        {
                            field = "email",
                            message = "Email đã tồn tại"
                        });
                    }
                    user.Email = req.Email;
                    user.TenDangNhap = req.Email;
                }
                user.HoTen = req.FullName;
                user.GioiTinh = req.Gender;
                user.NgaySinh = ToDateOnly(req.Dob);
                user.SoDienThoai = req.Phone;
                user.Avatar = req.Avatar;
                user.DiaChi = req.Address;
                user.UpdatedAt = DateTime.UtcNow;
            }

            // Cập nhật hồ sơ học tập
            sv.NganhId = req.NganhId;
            sv.KhoaTuyenSinhId = req.KhoaTuyenSinhId;
            if (req.Gpa.HasValue) sv.Gpa = req.Gpa.Value;
            if (req.TongTinChi.HasValue) sv.TongTinChi = req.TongTinChi.Value;
            sv.UpdatedAt = DateTime.UtcNow;

            // 🔥 TỰ ĐỘNG CHUYỂN "ĐÃ TỐT NGHIỆP" KHI TỔNG TÍN CHỈ >= 200
            if (user != null && sv.TongTinChi.HasValue && sv.TongTinChi.Value >= 200)
            {
                user.TrangThai = 2; // 2 = Đã tốt nghiệp
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ======================= 5. PUT /{id}/status =======================
        // Khóa/Mở khóa/Tốt nghiệp thủ công
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateSinhVienStatusRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sv = await _db.HoSoSinhViens
                .FirstOrDefaultAsync(s => s.SinhVienId == id);
            var user = sv != null
                ? await _db.NguoiDungs.FirstOrDefaultAsync(u => u.NguoiDungId == sv.NguoiDungId)
                : null;

            if (sv == null)
                return NotFound(new { message = "Không tìm thấy sinh viên" });

            if (user == null)
                return BadRequest(new { message = "Sinh viên không có tài khoản người dùng" });

            user.TrangThai = req.Status;
            user.UpdatedAt = DateTime.UtcNow;
            sv.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ======================= 6. GET /{id}/lop-hoc =======================
        // Danh sách lớp của sinh viên
        [HttpGet("{id:int}/lop-hoc")]
        public async Task<IActionResult> GetLopHoc(int id)
        {
            var exists = await _db.HoSoSinhViens
                .AnyAsync(s => s.SinhVienId == id);
            if (!exists)
                return NotFound(new { message = "Không tìm thấy sinh viên" });

            var data = await _db.SinhVienLops
                .Where(x => x.SinhVienId == id)
                .Include(x => x.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                .Include(x => x.LopHoc)
                    .ThenInclude(l => l.HocKy)
                .AsNoTracking()
                .Select(x => new
                {
                    lopHocId = x.LopHoc.LopHocId,
                    maLop = x.LopHoc.MaLop,
                    tenLop = x.LopHoc.TenLop,
                    monHoc = x.LopHoc.MonHoc != null ? x.LopHoc.MonHoc.TenMon : null,
                    hocKyId = x.LopHoc.HocKyId,
                    hocKy = x.LopHoc.HocKy != null ? x.LopHoc.HocKy.KiHoc : null,
                    namHoc = x.LopHoc.HocKy != null ? x.LopHoc.HocKy.NamHoc : null,
                    ngayBatDau = x.LopHoc.NgayBatDau,
                    ngayKetThuc = x.LopHoc.NgayKetThuc
                })
                .ToListAsync();

            return Ok(data);
        }

        // api/admin/sinh-vien/options
        [HttpGet("options")]
        public async Task<IActionResult> GetOptions()
        {
            var majors = await _db.Nganhs
                .AsNoTracking()
                .Select(n => new
                {
                    id = n.NganhId,
                    name = n.TenNganh
                })
                .ToListAsync();

            var cohorts = await _db.KhoaTuyenSinhs
                .AsNoTracking()
                .Select(k => new
                {
                    id = k.KhoaTuyenSinhId,
                    code = k.MaKhoaTuyenSinh,
                    name = k.TenKhoaTuyenSinh
                })
                .ToListAsync();

            return Ok(new { majors, cohorts });
        }

        [HttpPost("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var sv = await _db.HoSoSinhViens
                .FirstOrDefaultAsync(s => s.SinhVienId == id);
            if (sv == null)
                return NotFound(new { message = "Không tìm thấy sinh viên" });

            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(u => u.NguoiDungId == sv.NguoiDungId);
            if (user == null)
                return BadRequest(new { message = "Sinh viên không có tài khoản người dùng" });

            var newPassword = GenerateRandomPassword(10);
            user.HashMatKhau = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                username = user.TenDangNhap,
                password = newPassword
            });
        }

        // ======================= 7. DELETE /{id} =======================
        // Xóa sinh viên nếu không có dữ liệu liên quan
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sv = await _db.HoSoSinhViens.FirstOrDefaultAsync(s => s.SinhVienId == id);
            if (sv == null)
                return NotFound(new { message = "Không tìm thấy sinh viên" });

            var hasSinhVienLop = await _db.SinhVienLops.AnyAsync(x => x.SinhVienId == id);
            var hasDangKyTinChi = await _db.DangKyTinChis.AnyAsync(x => x.SinhVienId == id);
            var hasDiem = await _db.Diems.AnyAsync(x => x.SinhVienId == id);
            var hasDiemTP = await _db.DiemThanhPhans.AnyAsync(x => x.SinhVienId == id);
            var hasDiemChuyenCan = await _db.DiemChuyenCans.AnyAsync(x => x.SinhVienId == id);
            var hasDiemDanhCT = await _db.DiemDanhChiTiets.AnyAsync(x => x.SinhVienId == id);
            var hasBaiNop = await _db.BaiNops.AnyAsync(x => x.SinhVienId == id);
            var hasKetQua = await _db.KetQuaKiemTras.AnyAsync(x => x.SinhVienId == id);
            var hasTienDo = await _db.TienDos.AnyAsync(x => x.SinhVienId == id);
            var hasThongKe = await _db.ThongKeHocTaps.AnyAsync(x => x.SinhVienId == id);

            if (hasSinhVienLop || hasDangKyTinChi || hasDiem || hasDiemTP || hasDiemChuyenCan || hasDiemDanhCT || hasBaiNop || hasKetQua || hasTienDo || hasThongKe)
            {
                return BadRequest(new
                {
                    message = "Không thể xóa sinh viên vì đang có dữ liệu liên quan (lớp, điểm, điểm thành phần, điểm chuyên cần, điểm danh, bài nộp, kết quả kiểm tra, tiến độ, thống kê)"
                });
            }

            _db.HoSoSinhViens.Remove(sv);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
