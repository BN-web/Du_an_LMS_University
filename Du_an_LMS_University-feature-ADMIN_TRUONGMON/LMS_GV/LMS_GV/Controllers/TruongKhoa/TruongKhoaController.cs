using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LMS_GV.Models;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace LMS_GV.Controllers.TruongKhoa
{
    [ApiController]
    [Route("api/truong-khoa")]
    [Authorize(Roles = "TruongKhoa,Admin,Trưởng khoa")]
    public class TruongKhoaQuanLyController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<TruongKhoaQuanLyController> _logger;

        public TruongKhoaQuanLyController(AppDbContext db, ILogger<TruongKhoaQuanLyController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // =============== DTOs ==================

        public class TruongKhoaLecturerListQuery
        {
            public string? Q { get; set; }
            public int? NganhId { get; set; }
            public string? MonHoc { get; set; }   // có thể là "10" (MonHocId) hoặc keyword "Lập trình"
            public string? TrangThai { get; set; } // "hoat-dong" | "khong-hoat-dong"
            public int? KhoaId { get; set; }
        }

        public class TruongKhoaCreateBuoiHocRequest
        {
            [Required] public int LopHocId { get; set; }
            [Required] public int PhongHocId { get; set; }
            [Required] public DateTime ThoiGianBatDau { get; set; }
            [Required] public DateTime ThoiGianKetThuc { get; set; }
            public int? GiangVienId { get; set; }
            public string? GhiChu { get; set; }
            public bool OverrideConflicts { get; set; } = false;
            public string? OverrideReason { get; set; }
            public int? ActorNguoiDungId { get; set; }
        }

        public class TruongKhoaUpdateBuoiHocRequest
        {
            [Required] public int PhongHocId { get; set; }
            [Required] public DateTime ThoiGianBatDau { get; set; }
            [Required] public DateTime ThoiGianKetThuc { get; set; }
            public string? GhiChu { get; set; }
            public bool OverrideConflicts { get; set; } = false;
            public string? OverrideReason { get; set; }
            public int? ActorNguoiDungId { get; set; }
        }

        public class TruongKhoaCreateBuoiThiRequest
        {
            [Required] public int LopHocId { get; set; }
            [Required] public int PhongHocId { get; set; }
            [Required] public DateTime NgayThi { get; set; }
            public int? GiamThiId { get; set; }
            public string? HinhThuc { get; set; }
            public bool OverrideConflicts { get; set; } = false;
            public string? OverrideReason { get; set; }
            public int? ActorNguoiDungId { get; set; }
        }

        public class ClassListQuery
        {
            public string? Q { get; set; }
            public int? KhoaId { get; set; }
            public int? NganhId { get; set; }
            public int? MonHocId { get; set; }
            public int? GiangVienId { get; set; }
            public byte? Status { get; set; }
        }

        public class AssignGiangVienRequest
        {
            [Required] public int GiangVienId { get; set; }
            public bool OverrideConflicts { get; set; }
            public string? OverrideReason { get; set; }
            public int? ActorNguoiDungId { get; set; }
        }

        public class TruongKhoaCreateThongBaoRequest
        {
            [Required] public string TieuDe { get; set; } = string.Empty;
            public string? NoiDung { get; set; }
            [Required] public int NguoiDangId { get; set; }
            public string Target { get; set; } = "all"; // "toan-khoa"|"nganh"|"khoa-tuyen-sinh"|"mon"|"lop"|"giang-vien"|"sv"
            public int? GroupId { get; set; }
        }

        // ===================== NEW: OPTIONS ENDPOINTS =====================
        // Mục tiêu: trả { id, label } để frontend show tên thay vì nhập ID.

        // GET /api/truong-khoa/options/khoa?q=...
        [HttpGet("options/khoa")]
        public async Task<IActionResult> GetKhoaOptions([FromQuery] string? q)
        {
            try
            {
                var query = _db.Khoas.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var kw = q.Trim();
                    query = query.Where(x => (x.TenKhoa ?? "").Contains(kw) || (x.MaKhoa ?? "").Contains(kw));
                }

                var list = await query
                    .OrderBy(x => x.TenKhoa)
                    .Select(x => new
                    {
                        id = x.KhoaId,
                        label = (x.TenKhoa ?? "Khoa") + (x.MaKhoa != null ? $" ({x.MaKhoa})" : "")
                    })
                    .Take(300)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetKhoaOptions");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        // GET /api/truong-khoa/options/nganh?khoaId=...&q=...
        [HttpGet("options/nganh")]
        public async Task<IActionResult> GetNganhOptions([FromQuery] int? khoaId, [FromQuery] string? q)
        {
            try
            {
                var query = _db.Nganhs.AsNoTracking().AsQueryable();

                if (khoaId.HasValue) query = query.Where(x => x.KhoaId == khoaId.Value);

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var kw = q.Trim();
                    query = query.Where(x => (x.TenNganh ?? "").Contains(kw) || (x.MaNganh ?? "").Contains(kw));
                }

                var list = await query
                    .OrderBy(x => x.TenNganh)
                    .Select(x => new
                    {
                        id = x.NganhId,
                        label = (x.TenNganh ?? "Ngành") + (x.MaNganh != null ? $" ({x.MaNganh})" : "")
                    })
                    .Take(500)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetNganhOptions");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        // GET /api/truong-khoa/options/mon-hoc?q=...
        [HttpGet("options/mon-hoc")]
        public async Task<IActionResult> GetMonHocOptions([FromQuery] string? q)
        {
            try
            {
                var query = _db.MonHocs.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var kw = q.Trim();
                    query = query.Where(x => (x.TenMon ?? "").Contains(kw) || (x.MaMon ?? "").Contains(kw));
                }

                var list = await query
                    .OrderBy(x => x.TenMon)
                    .Select(x => new
                    {
                        id = x.MonHocId,
                        label = (x.TenMon ?? "Môn học") + (x.MaMon != null ? $" ({x.MaMon})" : "")
                    })
                    .Take(500)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMonHocOptions");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        // GET /api/truong-khoa/options/hoc-ky?q=...
        [HttpGet("options/hoc-ky")]
        public async Task<IActionResult> GetHocKyOptions([FromQuery] string? q)
        {
            try
            {
                var query = _db.HocKys.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var kw = q.Trim();
                    query = query.Where(x => (x.NamHoc ?? "").Contains(kw) || (x.KiHoc ?? "").Contains(kw));
                }

                var list = await query
                    .OrderByDescending(x => x.NamHoc)
                    .ThenBy(x => x.KiHoc)
                    .Select(x => new
                    {
                        id = x.HocKyId,
                        label = $"{x.NamHoc} - {x.KiHoc}"
                    })
                    .Take(200)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetHocKyOptions");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        // GET /api/truong-khoa/options/phong-hoc?q=...
        [HttpGet("options/phong-hoc")]
        public async Task<IActionResult> GetPhongHocOptions([FromQuery] string? q)
        {
            try
            {
                var query = _db.PhongHocs.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var kw = q.Trim();
                    query = query.Where(p =>
                        (p.TenPhong ?? "").Contains(kw) ||
                        (p.MaPhong ?? "").Contains(kw) ||
                        (p.DiaChi ?? "").Contains(kw));
                }

                var list = await query
                    .OrderBy(p => p.TenPhong)
                    .Select(p => new
                    {
                        id = p.PhongHocId,
                        label = (p.TenPhong ?? "Phòng") + (p.MaPhong != null ? $" ({p.MaPhong})" : "")
                    })
                    .Take(300)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPhongHocOptions FAILED | q={Q}", q);
                return StatusCode(500, new { message = "Lỗi máy chủ", error = ex.Message });
            }
        }

        // GET /api/truong-khoa/options/khoa-tuyen-sinh?q=...
        [HttpGet("options/khoa-tuyen-sinh")]
        public async Task<IActionResult> GetKhoaTuyenSinhOptions([FromQuery] string? q)
        {
            try
            {
                var query = _db.KhoaTuyenSinhs.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var kw = q.Trim();
                    query = query.Where(x => (x.TenKhoaTuyenSinh ?? "").Contains(kw) || (x.MaKhoaTuyenSinh ?? "").Contains(kw));
                }

                var list = await query
                    .OrderByDescending(x => x.TenKhoaTuyenSinh)
                    .Select(x => new
                    {
                        id = x.KhoaTuyenSinhId,
                        label = (x.TenKhoaTuyenSinh ?? "Khóa") + (x.MaKhoaTuyenSinh != null ? $" ({x.MaKhoaTuyenSinh})" : "")
                    })
                    .Take(200)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetKhoaTuyenSinhOptions");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        // GET /api/truong-khoa/options/lop-hoc?Q=...&KhoaId=...&NganhId=...&MonHocId=...&GiangVienId=...&Status=...
        [HttpGet("options/lop-hoc")]
        public async Task<IActionResult> GetLopHocOptions([FromQuery] ClassListQuery q)
        {
            try
            {
                var query = _db.LopHocs.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(q.Q))
                {
                    var kw = q.Q.Trim();
                    query = query.Where(l => (l.MaLop ?? "").Contains(kw) || (l.TenLop ?? "").Contains(kw));
                }
                if (q.KhoaId.HasValue) query = query.Where(l => l.KhoaId == q.KhoaId);
                if (q.NganhId.HasValue) query = query.Where(l => l.NganhId == q.NganhId);
                if (q.MonHocId.HasValue) query = query.Where(l => l.MonHocId == q.MonHocId);
                if (q.GiangVienId.HasValue) query = query.Where(l => l.GiangVienId == q.GiangVienId);
                if (q.Status.HasValue) query = query.Where(l => l.TrangThai == q.Status);

                // label đẹp hơn: TenLop (MaLop) — TenMon
                // Lấy dữ liệu trước, sau đó format string để tránh lỗi EF translation
                var list = await (
                    from l in query
                    join m in _db.MonHocs.AsNoTracking() on l.MonHocId equals m.MonHocId into monGroup
                    from m in monGroup.DefaultIfEmpty()
                    select new
                    {
                        id = l.LopHocId,
                        tenLop = l.TenLop ?? "Lớp",
                        maLop = l.MaLop,
                        tenMon = m != null ? m.TenMon : null
                    }
                )
                .OrderBy(x => x.tenLop)
                .ThenBy(x => x.maLop)
                .Take(500)
                .ToListAsync();

                // Format label sau khi query để tránh lỗi EF translation
                var formattedList = list.Select(x => new
                {
                    id = x.id,
                    label = x.tenLop +
                            (x.maLop != null ? " (" + x.maLop + ")" : "") +
                            (x.tenMon != null ? " — " + x.tenMon : "")
                })
                .OrderBy(x => x.label)
                .ToList();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetLopHocOptions FAILED | Q={Q}, KhoaId={KhoaId}, NganhId={NganhId}, MonHocId={MonHocId}, GiangVienId={GiangVienId}, Status={Status}",
                    q.Q, q.KhoaId, q.NganhId, q.MonHocId, q.GiangVienId, q.Status);
                return StatusCode(500, new { message = "Lỗi máy chủ", error = ex.Message });
            }
        }

        // GET /api/truong-khoa/options/giang-vien?Q=...&KhoaId=...&NganhId=...&MonHoc=...&TrangThai=...
        // label: "HoTen - Email (MS)"
        [HttpGet("options/giang-vien")]
        public async Task<IActionResult> GetGiangVienOptions([FromQuery] TruongKhoaLecturerListQuery q)
        {
            try
            {
                // Tách query để tránh nested Any() gây lỗi SQL
                // Bước 1: Lấy danh sách GiangVienId thỏa điều kiện NganhId/MonHoc (nếu có)
                List<int>? filteredGiangVienIds = null;

                if (q.NganhId.HasValue)
                {
                    var nganhId = q.NganhId.Value;
                    // Lấy danh sách LopHocId thuộc ngành trước
                    var lopHocIds = await _db.LopHocs
                        .AsNoTracking()
                        .Where(l => l.NganhId == nganhId)
                        .Select(l => l.LopHocId)
                        .ToListAsync();

                    // Lấy GiangVienId từ GiangVienLops
                    filteredGiangVienIds = await _db.GiangVienLops
                        .AsNoTracking()
                        .Where(gl => lopHocIds.Contains(gl.LopHocId))
                        .Select(gl => gl.GiangVienId)
                        .Distinct()
                        .ToListAsync();
                }

                if (!string.IsNullOrWhiteSpace(q.MonHoc))
                {
                    var monText = q.MonHoc.Trim();
                    List<int> monFilteredIds;

                    if (int.TryParse(monText, out var monHocId))
                    {
                        // Lấy danh sách LopHocId có MonHocId
                        var lopHocIds = await _db.LopHocs
                            .AsNoTracking()
                            .Where(l => l.MonHocId == monHocId)
                            .Select(l => l.LopHocId)
                            .ToListAsync();

                        monFilteredIds = await _db.GiangVienLops
                            .AsNoTracking()
                            .Where(gl => lopHocIds.Contains(gl.LopHocId))
                            .Select(gl => gl.GiangVienId)
                            .Distinct()
                            .ToListAsync();
                    }
                    else
                    {
                        // Lấy danh sách MonHocId thỏa keyword trước
                        var monHocIds = await _db.MonHocs
                            .AsNoTracking()
                            .Where(m => (m.TenMon != null && m.TenMon.Contains(monText)) || 
                                       (m.MaMon != null && m.MaMon.Contains(monText)))
                            .Select(m => m.MonHocId)
                            .ToListAsync();

                        // Lấy danh sách LopHocId có MonHocId trong danh sách
                        var lopHocIds = await _db.LopHocs
                            .AsNoTracking()
                            .Where(l => monHocIds.Contains(l.MonHocId))
                            .Select(l => l.LopHocId)
                            .ToListAsync();

                        monFilteredIds = await _db.GiangVienLops
                            .AsNoTracking()
                            .Where(gl => lopHocIds.Contains(gl.LopHocId))
                            .Select(gl => gl.GiangVienId)
                            .Distinct()
                            .ToListAsync();
                    }

                    // Giao của 2 filter
                    if (filteredGiangVienIds != null)
                    {
                        filteredGiangVienIds = filteredGiangVienIds.Intersect(monFilteredIds).ToList();
                    }
                    else
                    {
                        filteredGiangVienIds = monFilteredIds;
                    }
                }

                // Bước 2: Query chính với các filter đơn giản
                var baseQuery =
                    from gv in _db.GiangViens.AsNoTracking()
                    join nd in _db.NguoiDungs.AsNoTracking() on gv.NguoiDungId equals nd.NguoiDungId
                    select new { gv, nd };

                // Áp dụng filter GiangVienId từ bước 1 (nếu có)
                if (filteredGiangVienIds != null && filteredGiangVienIds.Any())
                {
                    baseQuery = baseQuery.Where(x => filteredGiangVienIds.Contains(x.gv.GiangVienId));
                }
                else if (filteredGiangVienIds != null && !filteredGiangVienIds.Any())
                {
                    // Không có giảng viên nào thỏa điều kiện
                    return Ok(new List<object>());
                }

                if (!string.IsNullOrWhiteSpace(q.Q))
                {
                    var kw = q.Q.Trim();
                    baseQuery = baseQuery.Where(x =>
                        (x.nd.HoTen != null && x.nd.HoTen.Contains(kw)) ||
                        (x.nd.Email != null && x.nd.Email.Contains(kw)) ||
                        (x.gv.Ms != null && x.gv.Ms.Contains(kw)) ||
                        (x.nd.TenDangNhap != null && x.nd.TenDangNhap.Contains(kw)));
                }

                // lọc khoa theo GiangVien.KhoaId
                if (q.KhoaId.HasValue)
                {
                    baseQuery = baseQuery.Where(x => x.gv.KhoaId == q.KhoaId.Value);
                }

                // lọc trạng thái theo NguoiDung.TrangThai
                if (!string.IsNullOrWhiteSpace(q.TrangThai))
                {
                    var tt = q.TrangThai.Trim().ToLower();
                    if (tt == "hoat-dong")
                    {
                        baseQuery = baseQuery.Where(x => x.nd.TrangThai == 1);
                    }
                    else if (tt == "khong-hoat-dong")
                    {
                        baseQuery = baseQuery.Where(x => x.nd.TrangThai != 1);
                    }
                }

                var list = await baseQuery
                    .OrderBy(x => x.nd.HoTen)
                    .Select(x => new
                    {
                        id = x.gv.GiangVienId,
                        label =
                            (x.nd.HoTen ?? "Giảng viên") +
                            (!string.IsNullOrWhiteSpace(x.nd.Email) ? $" - {x.nd.Email}" : "") +
                            (!string.IsNullOrWhiteSpace(x.gv.Ms) ? $" ({x.gv.Ms})" : "")
                    })
                    .Take(500)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetGiangVienOptions FAILED | Q={Q}, KhoaId={KhoaId}, NganhId={NganhId}, MonHoc={MonHoc}, TrangThai={TrangThai}",
                    q.Q, q.KhoaId, q.NganhId, q.MonHoc, q.TrangThai);
                return StatusCode(500, new { message = "Lỗi máy chủ", error = ex.Message });
            }
        }

        // ===================== DASHBOARD =====================

        [HttpGet("dashboard/summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var totalActiveClasses = await _db.LopHocs
                .CountAsync(l => (l.TrangThai == 1) ||
                                 (l.NgayBatDau.HasValue && l.NgayKetThuc.HasValue && l.NgayBatDau.Value <= today && today <= l.NgayKetThuc.Value));

            var totalLecturers = await _db.GiangViens.CountAsync();
            var totalStudents = await _db.HoSoSinhViens.CountAsync();

            var thongBaoGanDay = await (
                from tb in _db.ThongBaos.AsNoTracking()
                join nd in _db.NguoiDungs.AsNoTracking() on tb.NguoiDangId equals nd.NguoiDungId into gnd
                from nd in gnd.DefaultIfEmpty()
                orderby tb.NgayTao descending
                select new
                {
                    id = tb.ThongBaoId,
                    tieuDe = tb.TieuDe,
                    ngayTao = tb.NgayTao,
                    nguoiDang = nd != null ? nd.HoTen : null
                }
            ).Take(5).ToListAsync();

            var hoatDongGanDay = await _db.NhatKyHeThongs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    id = x.NhatKyHeThongId,
                    hanhDong = x.HanhDong,
                    thamChieu = x.ThamChieu,
                    nguoiDung = _db.NguoiDungs.Where(nd => nd.NguoiDungId == x.NguoiDungId).Select(nd => nd.HoTen).FirstOrDefault(),
                    createdAt = x.CreatedAt
                })
                .Take(10)
                .ToListAsync();

            var recentAssignments = await (
                from bkt in _db.BaiKiemTras.AsNoTracking()
                join l in _db.LopHocs.AsNoTracking() on bkt.LopHocId equals l.LopHocId
                where bkt.NgayKetThuc.HasValue && bkt.NgayKetThuc.Value >= DateTime.UtcNow.AddDays(-30)
                select new
                {
                    lopHocId = l.LopHocId,
                    tenLop = l.TenLop,
                    baiKiemTraId = bkt.BaiKiemTraId
                }
            ).ToListAsync();

            var completionChart = new List<object>();
            foreach (var item in recentAssignments)
            {
                var expected = await _db.SinhVienLops.CountAsync(s => s.LopHocId == item.lopHocId);
                var submitted = await _db.BaiNops.CountAsync(n => n.BaiKiemTraId == item.baiKiemTraId);
                double rate = expected > 0 ? Math.Round(submitted * 100.0 / expected, 2) : 0;
                completionChart.Add(new
                {
                    lopHocId = item.lopHocId,
                    tenLop = item.tenLop,
                    completionRate = rate
                });
            }

            return Ok(new
            {
                totals = new
                {
                    totalActiveClasses,
                    totalLecturers,
                    totalStudents
                },
                thongBaoGanDay,
                hoatDongGanDay,
                completionChart
            });
        }

        [HttpGet("dashboard/export/csv")]
        public async Task<IActionResult> ExportDashboardCsv()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var totalActiveClasses = await _db.LopHocs
                .CountAsync(l => (l.TrangThai == 1) ||
                                 (l.NgayBatDau.HasValue && l.NgayKetThuc.HasValue && l.NgayBatDau.Value <= today && today <= l.NgayKetThuc.Value));
            var totalLecturers = await _db.GiangViens.CountAsync();
            var totalStudents = await _db.HoSoSinhViens.CountAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Metric,Value");
            sb.AppendLine($"TotalActiveClasses,{totalActiveClasses}");
            sb.AppendLine($"TotalLecturers,{totalLecturers}");
            sb.AppendLine($"TotalStudents,{totalStudents}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"dashboard_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }

        [HttpGet("dashboard/export/pdf")]
        public async Task<IActionResult> ExportDashboardPdf()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var totalActiveClasses = await _db.LopHocs
                .CountAsync(l => (l.TrangThai == 1) ||
                                 (l.NgayBatDau.HasValue && l.NgayKetThuc.HasValue && l.NgayBatDau.Value <= today && today <= l.NgayKetThuc.Value));
            var totalLecturers = await _db.GiangViens.CountAsync();
            var totalStudents = await _db.HoSoSinhViens.CountAsync();

            var recentActivities = await _db.NhatKyHeThongs.AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new
                {
                    x.HanhDong,
                    x.ThamChieu,
                    at = x.CreatedAt
                })
                .ToListAsync();

            var fileName = $"dashboard_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

            QuestPDF.Settings.License = LicenseType.Community;
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("Trưởng Khoa – Dashboard").FontSize(20).SemiBold();
                        row.ConstantItem(120).AlignRight().Text(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
                    });
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text("Tổng quan").FontSize(16).SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });
                            table.Cell().Element(CellHeader).Text("Chỉ số");
                            table.Cell().Element(CellHeader).Text("Giá trị");

                            table.Cell().Element(CellBody).Text("Tổng số lớp đang hoạt động");
                            table.Cell().Element(CellBody).Text(totalActiveClasses.ToString());
                            table.Cell().Element(CellBody).Text("Tổng số giảng viên");
                            table.Cell().Element(CellBody).Text(totalLecturers.ToString());
                            table.Cell().Element(CellBody).Text("Tổng số sinh viên");
                            table.Cell().Element(CellBody).Text(totalStudents.ToString());
                        });

                        col.Item().Text("Hoạt động gần đây").FontSize(16).SemiBold();
                        col.Item().Column(list =>
                        {
                            foreach (var a in recentActivities)
                            {
                                list.Item().Text($"- {a.at:yyyy-MM-dd HH:mm}: {a.HanhDong} ({a.ThamChieu})");
                            }
                        });
                    });

                    page.Footer().AlignRight().Text("Generated by LMS");
                });
            });

            using var ms = new System.IO.MemoryStream();
            document.GeneratePdf(ms);
            var bytes = ms.ToArray();
            return File(bytes, "application/pdf", fileName);

            IContainer CellHeader(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
            IContainer CellBody(IContainer container) => container.PaddingVertical(4);
        }

        // ===================== THÔNG BÁO =====================

        [HttpPost("thong-bao")]
        public async Task<IActionResult> CreateThongBao([FromBody] TruongKhoaCreateThongBaoRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ndExists = await _db.NguoiDungs.AnyAsync(x => x.NguoiDungId == req.NguoiDangId);
            if (!ndExists)
                return BadRequest(new { field = "nguoiDangId", message = "Người gửi không tồn tại" });

            var tb = new ThongBao
            {
                TieuDe = req.TieuDe,
                NoiDung = req.NoiDung,
                NguoiDangId = req.NguoiDangId,
                NgayTao = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.ThongBaos.Add(tb);
            await _db.SaveChangesAsync();

            var receivers = await ResolveReceiversForDean(req.Target, req.GroupId);
            foreach (var ndId in receivers)
            {
                _db.ThongBaoNguoiDungs.Add(new ThongBaoNguoiDung
                {
                    ThongBaoId = tb.ThongBaoId,
                    NguoiDungId = ndId,
                    DaDoc = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return Ok(new { id = tb.ThongBaoId });
        }

        // ===================== LỚP HỌC =====================

        [HttpGet("lop-hoc")]
        public async Task<IActionResult> GetClasses([FromQuery] ClassListQuery q)
        {
            try
            {
                var query = _db.LopHocs.AsNoTracking().AsQueryable();
                if (!string.IsNullOrWhiteSpace(q.Q))
                {
                    var kw = q.Q.Trim();
                    query = query.Where(l => (l.MaLop ?? "").Contains(kw) || (l.TenLop ?? "").Contains(kw));
                }
                if (q.KhoaId.HasValue) query = query.Where(l => l.KhoaId == q.KhoaId);
                if (q.NganhId.HasValue) query = query.Where(l => l.NganhId == q.NganhId);
                if (q.MonHocId.HasValue) query = query.Where(l => l.MonHocId == q.MonHocId);
                if (q.GiangVienId.HasValue) query = query.Where(l => l.GiangVienId == q.GiangVienId);
                if (q.Status.HasValue) query = query.Where(l => l.TrangThai == q.Status);

                var list = await (
                    from l in query
                    join m in _db.MonHocs on l.MonHocId equals m.MonHocId
                    join gv in _db.GiangViens on l.GiangVienId equals gv.GiangVienId into ggv
                    from gv in ggv.DefaultIfEmpty()
                    join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId into gnd
                    from nd in gnd.DefaultIfEmpty()
                    select new
                    {
                        id = l.LopHocId,
                        maLop = l.MaLop,
                        tenLop = l.TenLop,
                        mon = m.TenMon,
                        khoaId = l.KhoaId,
                        nganhId = l.NganhId,
                        giangVien = nd != null ? nd.HoTen : null,
                        siSoToiDa = l.SiSoToiDa,
                        trangThai = l.TrangThai
                    }
                ).ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClasses");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        [HttpGet("lop-hoc/{id:int}")]
        public async Task<IActionResult> GetClassDetail(int id)
        {
            try
            {
                var lop = await _db.LopHocs.AsNoTracking()
                    .Include(l => l.MonHoc)
                    .Include(l => l.HocKy)
                    .Include(l => l.Nganh)
                    .FirstOrDefaultAsync(l => l.LopHocId == id);
                if (lop == null)
                    return NotFound(new { message = "Không tìm thấy lớp học" });

                var gvHoTen = await (
                    from l in _db.LopHocs
                    join gv in _db.GiangViens on l.GiangVienId equals gv.GiangVienId
                    join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                    where l.LopHocId == id
                    select nd.HoTen
                ).FirstOrDefaultAsync();

                var totalStudents = await _db.SinhVienLops.CountAsync(sl => sl.LopHocId == id);

                return Ok(new
                {
                    id = lop.LopHocId,
                    maLop = lop.MaLop,
                    tenLop = lop.TenLop,
                    mon = lop.MonHoc?.TenMon,
                    nganh = lop.Nganh?.TenNganh,
                    nganhId = lop.NganhId,
                    hocKy = lop.HocKy?.KiHoc,
                    giangVien = gvHoTen,
                    siSoToiDa = lop.SiSoToiDa,
                    trangThai = lop.TrangThai,
                    ngayBatDau = lop.NgayBatDau,
                    ngayKetThuc = lop.NgayKetThuc,
                    totalStudents
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClassDetail {id}", id);
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        [HttpGet("lop-hoc/{id:int}/sinh-vien")]
        public async Task<IActionResult> GetClassStudents(int id)
        {
            try
            {
                var exists = await _db.LopHocs.AnyAsync(l => l.LopHocId == id);
                if (!exists) return NotFound(new { message = "Không tìm thấy lớp học" });

                var list = await (
                    from svlop in _db.SinhVienLops.AsNoTracking()
                    join sv in _db.HoSoSinhViens on svlop.SinhVienId equals sv.SinhVienId
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    join ng in _db.Nganhs on sv.NganhId equals ng.NganhId into gng
                    from ng in gng.DefaultIfEmpty()
                    join d in _db.Diems on new { svlop.SinhVienId, LopHocId = id } equals new { d.SinhVienId, d.LopHocId } into gd
                    from d in gd.DefaultIfEmpty()
                    where svlop.LopHocId == id
                    select new
                    {
                        mssv = sv.Mssv,
                        hoTen = nd.HoTen,
                        nganh = ng != null ? ng.TenNganh : null,
                        tongDiem = d != null ? d.DiemTrungBinhMon : (decimal?)null
                    }
                ).ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClassStudents {id}", id);
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        [HttpGet("lop-hoc/{id:int}/buoi-hoc")]
        public async Task<IActionResult> GetBuoiHoc(int id)
        {
            try
            {
                var exists = await _db.LopHocs.AnyAsync(l => l.LopHocId == id);
                if (!exists) return NotFound(new { message = "Không tìm thấy lớp học" });

                var data = await (
                    from bh in _db.BuoiHocs
                    join ph in _db.PhongHocs on bh.PhongHocId equals ph.PhongHocId into gph
                    from ph in gph.DefaultIfEmpty()
                    where bh.LopHocId == id
                    select new
                    {
                        buoiHocId = bh.BuoiHocId,
                        thu = bh.Thứ,
                        thoiGianBatDau = bh.ThoiGianBatDau,
                        thoiGianKetThuc = bh.ThoiGianKetThuc,
                        phongHoc = ph != null ? ph.TenPhong : null,
                        maPhong = ph != null ? ph.MaPhong : null,
                        diaChi = ph != null ? ph.DiaChi : null,
                        trangThai = bh.TrangThai
                    }
                ).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetBuoiHoc {id}", id);
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        [HttpGet("lop-hoc/{id:int}/buoi-thi")]
        public async Task<IActionResult> GetBuoiThi(int id)
        {
            try
            {
                var exists = await _db.LopHocs.AnyAsync(l => l.LopHocId == id);
                if (!exists) return NotFound(new { message = "Không tìm thấy lớp học" });

                var data = await (
                    from bt in _db.BuoiThis
                    join ph in _db.PhongHocs on bt.PhongHocId equals ph.PhongHocId into gph
                    from ph in gph.DefaultIfEmpty()
                    where bt.LopHocId == id
                    select new
                    {
                        buoiThiId = bt.BuoiThiId,
                        ngayThi = bt.NgayThi,
                        thu = bt.Thứ,
                        gioBatDau = bt.GioBatDau,
                        gioKetThuc = bt.GioKetThuc,
                        hinhThuc = bt.HinhThuc,
                        phongHoc = ph != null ? ph.TenPhong : null,
                        maPhong = ph != null ? ph.MaPhong : null,
                        diaChi = ph != null ? ph.DiaChi : null
                    }
                ).ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetBuoiThi {id}", id);
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        [HttpPost("lop-hoc/{id:int}/assign-giang-vien")]
        public async Task<IActionResult> AssignGiangVien(int id, [FromBody] AssignGiangVienRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var lop = await _db.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == id);
                if (lop == null) return NotFound(new { message = "Không tìm thấy lớp học" });
                var gv = await _db.GiangViens.FirstOrDefaultAsync(g => g.GiangVienId == req.GiangVienId);
                if (gv == null)
                {
                    _logger.LogWarning("AssignGiangVien: giangVienId not found {gvId}", req.GiangVienId);
                    return BadRequest(new { field = "giangVienId", message = "Giảng viên không tồn tại" });
                }

                var classSessions = await _db.BuoiHocs.Where(b => b.LopHocId == id).ToListAsync();
                var otherClassIds = await _db.GiangVienLops.Where(gl => gl.GiangVienId == req.GiangVienId).Select(gl => gl.LopHocId).ToListAsync();
                var otherSessions = await _db.BuoiHocs.Where(b => otherClassIds.Contains(b.LopHocId)).ToListAsync();
                var conflicts = new List<object>();
                foreach (var cs in classSessions)
                {
                    var cfs = otherSessions
                        .Where(b => cs.ThoiGianBatDau < b.ThoiGianKetThuc && cs.ThoiGianKetThuc > b.ThoiGianBatDau)
                        .Select(b => new { b.BuoiHocId, b.LopHocId, b.ThoiGianBatDau, b.ThoiGianKetThuc });
                    conflicts.AddRange(cfs);
                }

                if (conflicts.Any() && !req.OverrideConflicts)
                    return Conflict(new { message = "Xung đột lịch giảng viên", conflicts });

                lop.GiangVienId = req.GiangVienId;
                lop.UpdatedAt = DateTime.UtcNow;
                var mapping = await _db.GiangVienLops.FirstOrDefaultAsync(x => x.LopHocId == id && x.GiangVienId == req.GiangVienId);
                if (mapping == null)
                {
                    _db.GiangVienLops.Add(new GiangVienLop
                    {
                        LopHocId = id,
                        GiangVienId = req.GiangVienId,
                        NgayPhanCong = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _db.SaveChangesAsync();

                if (conflicts.Any() && req.OverrideConflicts)
                {
                    _db.NhatKyHeThongs.Add(new NhatKyHeThong
                    {
                        NguoiDungId = req.ActorNguoiDungId,
                        HanhDong = "Override phân công giảng viên",
                        ThamChieu = $"LopHoc:{id};GiangVien:{req.GiangVienId};Reason:{req.OverrideReason}",
                        CreatedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }

                return Ok(new { message = "Phân công thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AssignGiangVien {id}", id);
                return StatusCode(500, new { message = "Lỗi máy chủ", detail = ex.Message });
            }
        }

        // ===================== LỊCH DẠY / LỊCH THI =====================

        [HttpPost("lich-day")]
        public async Task<IActionResult> CreateBuoiHoc([FromBody] TruongKhoaCreateBuoiHocRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var lop = await _db.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == req.LopHocId);
                if (lop == null) return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });
                var roomExists = await _db.PhongHocs.AnyAsync(r => r.PhongHocId == req.PhongHocId);
                if (!roomExists) return BadRequest(new { field = "phongHocId", message = "Phòng học không tồn tại" });

                var conflicts = new List<object>();
                if (req.PhongHocId != 0)
                {
                    var rc = await _db.BuoiHocs.Where(b => b.PhongHocId == req.PhongHocId)
                        .Where(b => req.ThoiGianBatDau < b.ThoiGianKetThuc && req.ThoiGianKetThuc > b.ThoiGianBatDau)
                        .Select(b => new { b.BuoiHocId, b.LopHocId, b.ThoiGianBatDau, b.ThoiGianKetThuc })
                        .ToListAsync();
                    conflicts.AddRange(rc);
                }

                var gvId = req.GiangVienId ?? lop.GiangVienId;
                if (gvId.HasValue)
                {
                    var otherClassIds = await _db.GiangVienLops.Where(gl => gl.GiangVienId == gvId.Value).Select(gl => gl.LopHocId).ToListAsync();
                    var gc = await _db.BuoiHocs.Where(b => otherClassIds.Contains(b.LopHocId))
                        .Where(b => req.ThoiGianBatDau < b.ThoiGianKetThuc && req.ThoiGianKetThuc > b.ThoiGianBatDau)
                        .Select(b => new { b.BuoiHocId, b.LopHocId, b.ThoiGianBatDau, b.ThoiGianKetThuc })
                        .ToListAsync();
                    conflicts.AddRange(gc);
                }

                if (conflicts.Any() && !req.OverrideConflicts)
                    return Conflict(new { message = "Xung đột phòng hoặc giảng viên", conflicts });

                var e = new BuoiHoc
                {
                    LopHocId = req.LopHocId,
                    PhongHocId = req.PhongHocId,
                    Thứ = req.ThoiGianBatDau.DayOfWeek.ToString(),
                    ThoiGianBatDau = req.ThoiGianBatDau,
                    ThoiGianKetThuc = req.ThoiGianKetThuc,
                    TrangThai = 1,
                    GhiChu = req.GhiChu,
                    CreatedAt = DateTime.UtcNow,
                    SoBuoi = 1
                };

                _db.BuoiHocs.Add(e);
                await _db.SaveChangesAsync();

                if (req.OverrideConflicts)
                {
                    _db.NhatKyHeThongs.Add(new NhatKyHeThong
                    {
                        NguoiDungId = req.ActorNguoiDungId,
                        HanhDong = "Override tạo lịch dạy",
                        ThamChieu = $"BuoiHoc:{e.BuoiHocId};LopHoc:{req.LopHocId};Reason:{req.OverrideReason}",
                        CreatedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }

                return Ok(new { id = e.BuoiHocId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateBuoiHoc");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        [HttpPost("lich-thi")]
        public async Task<IActionResult> CreateBuoiThi([FromBody] TruongKhoaCreateBuoiThiRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var exists = await _db.LopHocs.AnyAsync(l => l.LopHocId == req.LopHocId);
                if (!exists) return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });
                var roomExists = await _db.PhongHocs.AnyAsync(p => p.PhongHocId == req.PhongHocId);
                if (!roomExists) return BadRequest(new { field = "phongHocId", message = "Phòng học không tồn tại" });

                var conflicts = await _db.BuoiThis
                    .Where(bt => bt.PhongHocId == req.PhongHocId && bt.NgayThi.Date == req.NgayThi.Date)
                    .Select(bt => new { bt.BuoiThiId, bt.LopHocId, bt.NgayThi })
                    .ToListAsync();
                if (conflicts.Any() && !req.OverrideConflicts)
                    return Conflict(new { message = "Xung đột phòng thi", conflicts });

                var e = new BuoiThi
                {
                    LopHocId = req.LopHocId,
                    PhongHocId = req.PhongHocId,
                    NgayThi = req.NgayThi.Date,
                    Thứ = req.NgayThi.DayOfWeek.ToString(),
                    HinhThuc = req.HinhThuc,
                    GiamThiId = req.GiamThiId,
                    CreatedAt = DateTime.UtcNow
                };

                _db.BuoiThis.Add(e);
                await _db.SaveChangesAsync();

                if (req.OverrideConflicts)
                {
                    _db.NhatKyHeThongs.Add(new NhatKyHeThong
                    {
                        NguoiDungId = req.ActorNguoiDungId,
                        HanhDong = "Override tạo lịch thi",
                        ThamChieu = $"BuoiThi:{e.BuoiThiId};LopHoc:{req.LopHocId};Reason:{req.OverrideReason}",
                        CreatedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }

                return Ok(new { id = e.BuoiThiId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateBuoiThi");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        // ===================== PRIVATE HELPERS =====================

        private async Task<List<int>> ResolveReceiversForDean(string target, int? groupId)
        {
            target = (target ?? "all").ToLower().Trim();
            var result = new List<int>();

            if (target is "all" or "toan-khoa")
            {
                result = await _db.NguoiDungs.Select(x => x.NguoiDungId).ToListAsync();
            }
            else if (target == "nganh" && groupId.HasValue)
            {
                int nganhId = groupId.Value;

                var svNguoiDung = await (
                    from sv in _db.HoSoSinhViens
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    where sv.NganhId == nganhId
                    select nd.NguoiDungId
                ).ToListAsync();

                var gvNguoiDung = await (
                    from gl in _db.GiangVienLops
                    join l in _db.LopHocs on gl.LopHocId equals l.LopHocId
                    join gv in _db.GiangViens on gl.GiangVienId equals gv.GiangVienId
                    join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                    where l.NganhId == nganhId
                    select nd.NguoiDungId
                ).Distinct().ToListAsync();

                result = svNguoiDung.Concat(gvNguoiDung).Distinct().ToList();
            }
            else if (target == "khoa-tuyen-sinh" && groupId.HasValue)
            {
                int cohortId = groupId.Value;
                result = await (
                    from sv in _db.HoSoSinhViens
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    where sv.KhoaTuyenSinhId == cohortId
                    select nd.NguoiDungId
                ).ToListAsync();
            }
            else if (target == "mon" && groupId.HasValue)
            {
                int monHocId = groupId.Value;

                var svNguoiDung = await (
                    from svlop in _db.SinhVienLops
                    join sv in _db.HoSoSinhViens on svlop.SinhVienId equals sv.SinhVienId
                    join l in _db.LopHocs on svlop.LopHocId equals l.LopHocId
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    where l.MonHocId == monHocId
                    select nd.NguoiDungId
                ).ToListAsync();

                var gvNguoiDung = await (
                    from gl in _db.GiangVienLops
                    join l in _db.LopHocs on gl.LopHocId equals l.LopHocId
                    join gv in _db.GiangViens on gl.GiangVienId equals gv.GiangVienId
                    join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                    where l.MonHocId == monHocId
                    select nd.NguoiDungId
                ).ToListAsync();

                result = svNguoiDung.Concat(gvNguoiDung).Distinct().ToList();
            }
            else if (target == "lop" && groupId.HasValue)
            {
                int lopHocId = groupId.Value;

                var svNguoiDung = await (
                    from svlop in _db.SinhVienLops
                    join sv in _db.HoSoSinhViens on svlop.SinhVienId equals sv.SinhVienId
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    where svlop.LopHocId == lopHocId
                    select nd.NguoiDungId
                ).ToListAsync();

                var gvNguoiDung = await (
                    from gl in _db.GiangVienLops
                    join gv in _db.GiangViens on gl.GiangVienId equals gv.GiangVienId
                    join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                    where gl.LopHocId == lopHocId
                    select nd.NguoiDungId
                ).ToListAsync();

                result = svNguoiDung.Concat(gvNguoiDung).Distinct().ToList();
            }
            else if (target == "giang-vien" && groupId.HasValue)
            {
                int gvId = groupId.Value;
                var ndId = await _db.GiangViens
                    .Where(g => g.GiangVienId == gvId)
                    .Select(g => g.NguoiDungId)
                    .FirstOrDefaultAsync();

                if (ndId != 0) result.Add(ndId);
            }
            else if (target == "sv" && groupId.HasValue)
            {
                int svId = groupId.Value;
                var ndId = await _db.HoSoSinhViens
                    .Where(s => s.SinhVienId == svId)
                    .Select(s => s.NguoiDungId)
                    .FirstOrDefaultAsync();

                if (ndId != 0) result.Add(ndId);
            }

            return result.Distinct().ToList();
        }
    }
}
