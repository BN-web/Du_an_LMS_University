using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LMS_GV.Models;
using LMS_GV.Models.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/giang-vien")]
    [Authorize(Roles = "Admin")]
    public class GiangVienController : ControllerBase
    {
        private readonly AppDbContext _db;

        public GiangVienController(AppDbContext db)
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

        public class GiangVienListQuery
        {
            public string? Search { get; set; }      // tên hoặc mã
            public string? ChuyenMon { get; set; }
            public string? ChucVu { get; set; }
            public int? KhoaId { get; set; }
            public byte? Status { get; set; }        // 0=bị khóa,1=đang hoạt động,3=dừng hoạt động
        }

        public class CreateGiangVienRequest
        {
            [Required]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã giảng viên phải gồm đúng 6 ký tự số")]
            public string Code { get; set; } = string.Empty; // MS

            [Required]
            public string FullName { get; set; } = string.Empty;

            public string? ChuyenMon { get; set; }
            public string? HocVi { get; set; }
            public string? ChucVu { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            public string? Phone { get; set; }
            public string? Gender { get; set; }
            public string? Address { get; set; }
            public string? Avatar { get; set; }
            public DateTime? Dob { get; set; }

            public int? KhoaId { get; set; }

            public byte? Status { get; set; } // default 1

            public string? Password { get; set; }
            
            public int? VaiTroId { get; set; }
        }

        public class UpdateGiangVienRequest
        {
            [Required]
            public string FullName { get; set; } = string.Empty;

            public string? ChuyenMon { get; set; }
            public string? HocVi { get; set; }
            public string? ChucVu { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            public string? Phone { get; set; }
            public string? Gender { get; set; }
            public string? Address { get; set; }
            public string? Avatar { get; set; }
            public DateTime? Dob { get; set; }

            public int? KhoaId { get; set; }

            [StringLength(6, MinimumLength = 6)]
            public string? Code { get; set; } // cho phép đổi mã nếu cần
        }

        public class UpdateGiangVienStatusRequest
        {
            [Required]
            public byte Status { get; set; } // 0=bị khóa,1=đang hoạt động,3=dừng hoạt động
        }

        // ======================= 1. GET / =======================
        // Danh sách + tìm kiếm + filter
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] GiangVienListQuery queryModel)
        {
            var query = _db.GiangViens
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Search))
            {
                var kw = queryModel.Search.Trim();
                query = query.Where(gv =>
                    (gv.Ms ?? "").Contains(kw) ||
                    _db.NguoiDungs.Any(nd =>
                        nd.NguoiDungId == gv.NguoiDungId &&
                        (nd.HoTen ?? "").Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(queryModel.ChuyenMon))
            {
                var cm = queryModel.ChuyenMon.Trim();
                query = query.Where(gv => (gv.ChuyenMon ?? "").Contains(cm));
            }

            if (!string.IsNullOrWhiteSpace(queryModel.ChucVu))
            {
                var cv = queryModel.ChucVu.Trim();
                query = query.Where(gv => (gv.ChucVu ?? "").Contains(cv));
            }

            if (queryModel.KhoaId.HasValue)
            {
                query = query.Where(gv => gv.KhoaId == queryModel.KhoaId.Value);
            }

            if (queryModel.Status.HasValue)
            {
                var st = queryModel.Status.Value;
                query = query.Where(gv =>
                    _db.NguoiDungs.Any(nd =>
                        nd.NguoiDungId == gv.NguoiDungId &&
                        nd.TrangThai == st));
            }

            var list = await query
                .OrderByDescending(gv => gv.CreatedAt)
                .Select(gv => new
                {
                    id = gv.GiangVienId,
                    code = gv.Ms,
                    fullName = _db.NguoiDungs
                        .Where(nd => nd.NguoiDungId == gv.NguoiDungId)
                        .Select(nd => nd.HoTen)
                        .FirstOrDefault(),
                    specialization = gv.ChuyenMon,
                    degree = gv.HocVi,
                    position = gv.ChucVu,
                    facultyId = gv.KhoaId,
                    facultyName = _db.Khoas
                        .Where(k => k.KhoaId == gv.KhoaId)
                        .Select(k => k.TenKhoa)
                        .FirstOrDefault(),
                    createdAt = gv.CreatedAt,
                    status = _db.NguoiDungs
                        .Where(nd => nd.NguoiDungId == gv.NguoiDungId)
                        .Select(nd => nd.TrangThai)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(list);
        }

        // ======================= 2. GET /{id} =======================
        // Chi tiết hồ sơ + tổng lớp phụ trách
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var gv = await _db.GiangViens
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.GiangVienId == id);

            if (gv == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var nd = await _db.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NguoiDungId == gv.NguoiDungId);

            var k = await _db.Khoas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.KhoaId == gv.KhoaId);

            var totalClasses = await _db.GiangVienLops
                .Where(gl => gl.GiangVienId == id)
                .Select(gl => gl.LopHocId)
                .Distinct()
                .CountAsync();

            var result = new
            {
                id = gv.GiangVienId,
                code = gv.Ms,
                fullName = nd?.HoTen,
                specialization = gv.ChuyenMon,
                degree = gv.HocVi,
                position = gv.ChucVu,
                email = nd?.Email,
                phone = nd?.SoDienThoai,
                gender = nd?.GioiTinh,
                address = nd?.DiaChi,
                avatar = nd?.Avatar,
                dob = nd?.NgaySinh,
                facultyId = gv.KhoaId,
                facultyName = k?.TenKhoa,
                status = nd?.TrangThai,
                createdAt = gv.CreatedAt,
                updatedAt = gv.UpdatedAt,
                totalClasses
            };

            return Ok(result);
        }

        // ======================= 3. POST / =======================
        // Thêm giảng viên
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGiangVienRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check mã GV trùng
            var existsCode = await _db.GiangViens
                .AnyAsync(g => g.Ms == req.Code);
            if (existsCode)
            {
                return Conflict(new
                {
                    field = "code",
                    message = "Mã giảng viên đã tồn tại"
                });
            }

            // Email bắt buộc và không trùng
            if (string.IsNullOrWhiteSpace(req.Email))
            {
                return BadRequest(new { field = "email", message = "Email là bắt buộc" });
            }
            var existsEmail = await _db.NguoiDungs
                .AnyAsync(u => u.Email == req.Email || u.TenDangNhap == req.Email);
            if (existsEmail)
            {
                return Conflict(new { field = "email", message = "Email đã tồn tại" });
            }

            // Tạo tài khoản người dùng (VaiTro_id = 2: Giảng viên)
            var initialPassword = GenerateRandomPassword(10);
            var roleId = req.VaiTroId ?? (string.Equals(req.ChucVu ?? "", "Trưởng khoa", StringComparison.OrdinalIgnoreCase) ? 3 : 2);
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
                TrangThai = req.Status ?? 1,
                VaiTroId = roleId,
                CreatedAt = DateTime.UtcNow,
                HashMatKhau = BCrypt.Net.BCrypt.HashPassword(initialPassword)
            };
            _db.NguoiDungs.Add(user);
            await _db.SaveChangesAsync();

            var gv = new GiangVien
            {
                Ms = req.Code,
                NguoiDungId = user.NguoiDungId,
                ChucVu = req.ChucVu,
                KhoaId = req.KhoaId,
                HocVi = req.HocVi,
                ChuyenMon = req.ChuyenMon,
                CreatedAt = DateTime.UtcNow
            };

            _db.GiangViens.Add(gv);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetail), new { id = gv.GiangVienId }, new
            {
                id = gv.GiangVienId,
                code = gv.Ms,
                username = user.TenDangNhap,
                initialPassword
            });
        }

        // ======================= 4. PUT /{id} =======================
        // Sửa giảng viên
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGiangVienRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gv = await _db.GiangViens
                .FirstOrDefaultAsync(g => g.GiangVienId == id);
            var user = gv != null
                ? await _db.NguoiDungs.FirstOrDefaultAsync(u => u.NguoiDungId == gv.NguoiDungId)
                : null;

            if (gv == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            // Sửa mã nếu có
            if (!string.IsNullOrWhiteSpace(req.Code) && req.Code != gv.Ms)
            {
                var existsCode = await _db.GiangViens
                    .AnyAsync(g => g.Ms == req.Code && g.GiangVienId != id);
                if (existsCode)
                {
                    return Conflict(new
                    {
                        field = "code",
                        message = "Mã giảng viên đã tồn tại"
                    });
                }

                gv.Ms = req.Code;
            }

            // Cập nhật user
            if (user != null)
            {
                // Đổi email -> kiểm tra trùng và đồng bộ tên đăng nhập theo email
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
                user.DiaChi = req.Address;
                user.Avatar = req.Avatar;
                user.UpdatedAt = DateTime.UtcNow;
            }

            // Cập nhật giảng viên
            gv.ChuyenMon = req.ChuyenMon;
            gv.HocVi = req.HocVi;
            gv.ChucVu = req.ChucVu;
            gv.KhoaId = req.KhoaId;
            gv.UpdatedAt = DateTime.UtcNow;
            if (user != null && !string.IsNullOrWhiteSpace(req.ChucVu))
            {
                var newRoleId = string.Equals(req.ChucVu ?? "", "Trưởng khoa", StringComparison.OrdinalIgnoreCase) ? 3 : 2;
                if ((user.VaiTroId ?? 0) != newRoleId)
                {
                    user.VaiTroId = newRoleId;
                }
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var gv = await _db.GiangViens
                .FirstOrDefaultAsync(g => g.GiangVienId == id);
            if (gv == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(u => u.NguoiDungId == gv.NguoiDungId);
            if (user == null)
                return BadRequest(new { message = "Giảng viên không có tài khoản người dùng" });

            var newPassword = GenerateRandomPassword(10);
            user.HashMatKhau = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { username = user.TenDangNhap, password = newPassword });
        }

        // ======================= 5. PUT /{id}/status =======================
        // Khóa/Mở/Dừng hoạt động
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateGiangVienStatusRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gv = await _db.GiangViens
                .FirstOrDefaultAsync(g => g.GiangVienId == id);
            var user = gv != null
                ? await _db.NguoiDungs.FirstOrDefaultAsync(u => u.NguoiDungId == gv.NguoiDungId)
                : null;

            if (gv == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            if (user == null)
                return BadRequest(new { message = "Giảng viên không có tài khoản người dùng" });

            user.TrangThai = req.Status;
            user.UpdatedAt = DateTime.UtcNow;
            gv.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ======================= 6. GET /{id}/lop-hoc =======================
        // Danh sách lớp giảng viên phụ trách
        [HttpGet("{id:int}/lop-hoc")]
        public async Task<IActionResult> GetLopHoc(int id)
        {
            var exists = await _db.GiangViens
                .AnyAsync(g => g.GiangVienId == id);
            if (!exists)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var data = await _db.GiangVienLops
                .Where(gl => gl.GiangVienId == id)
                .Include(gl => gl.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                .Include(gl => gl.LopHoc)
                    .ThenInclude(l => l.HocKy)
                .AsNoTracking()
                .Select(gl => new
                {
                    lopHocId = gl.LopHocId,
                    maLop = gl.LopHoc.MaLop,
                    tenLop = gl.LopHoc.TenLop,
                    monHoc = gl.LopHoc.MonHoc != null ? gl.LopHoc.MonHoc.TenMon : null,
                    hocKyId = gl.LopHoc.HocKyId,
                    hocKy = gl.LopHoc.HocKy != null ? gl.LopHoc.HocKy.KiHoc : null,
                    namHoc = gl.LopHoc.HocKy != null ? gl.LopHoc.HocKy.NamHoc : null,
                    soTinChi = gl.LopHoc.SoTinChi,
                    siSo = gl.LopHoc.SiSo,
                    ngayBatDau = gl.LopHoc.NgayBatDau,
                    ngayKetThuc = gl.LopHoc.NgayKetThuc
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.GiangVienId == id);
            if (gv == null)
                return NotFound(new { message = "Không tìm thấy giảng viên" });

            var hasAssignments = await _db.GiangVienLops.AnyAsync(x => x.GiangVienId == id);
            var hasPrimaryClasses = await _db.LopHocs.AnyAsync(l => l.GiangVienId == id);
            var hasProctorSessions = await _db.BuoiThis.AnyAsync(b => b.GiamThiId == id);

            if (hasAssignments || hasPrimaryClasses || hasProctorSessions)
            {
                return BadRequest(new { message = "Không thể xóa giảng viên vì đang có dữ liệu liên quan (phân công lớp, lớp phụ trách, buổi thi)" });
            }

            _db.GiangViens.Remove(gv);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
