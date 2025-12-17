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
    [Authorize(Roles = "TruongMon,TruongKhoa")]
    public class TruongKhoaQuanLyController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TruongKhoaQuanLyController(AppDbContext db)
        {
            _db = db;
        }

        public class TruongKhoaLecturerListQuery
        {
            public string? Q { get; set; }
            public int? NganhId { get; set; }
            public string? MonHoc { get; set; }
            public string? TrangThai { get; set; } // "hoat-dong" | "khong-hoat-dong"
            public int? KhoaId { get; set; }
        }

        public class TruongKhoaCreateBuoiHocRequest
        {
            [Required]
            public int LopHocId { get; set; }
            [Required]
            public int PhongHocId { get; set; }
            [Required]
            public DateTime ThoiGianBatDau { get; set; }
            [Required]
            public DateTime ThoiGianKetThuc { get; set; }
            public int? GiangVienId { get; set; }
            public string? GhiChu { get; set; }
            public bool OverrideConflicts { get; set; } = false;
            public string? OverrideReason { get; set; }
            public int? ActorNguoiDungId { get; set; }
        }

        public class TruongKhoaUpdateBuoiHocRequest
        {
            [Required]
            public int PhongHocId { get; set; }
            [Required]
            public DateTime ThoiGianBatDau { get; set; }
            [Required]
            public DateTime ThoiGianKetThuc { get; set; }
            public string? GhiChu { get; set; }
            public bool OverrideConflicts { get; set; } = false;
            public string? OverrideReason { get; set; }
            public int? ActorNguoiDungId { get; set; }
        }

        public class TruongKhoaCreateBuoiThiRequest
        {
            [Required]
            public int LopHocId { get; set; }
            [Required]
            public int PhongHocId { get; set; }
            [Required]
            public DateTime NgayThi { get; set; }
            public int? GiamThiId { get; set; }
            public string? HinhThuc { get; set; }
            public bool OverrideConflicts { get; set; } = false;
            public string? OverrideReason { get; set; }
            public int? ActorNguoiDungId { get; set; }
        }

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

        [HttpGet("giang-vien")]
        public async Task<IActionResult> GetLecturers([FromQuery] TruongKhoaLecturerListQuery query)
        {
            var baseQuery = _db.GiangViens.Include(g => g.NguoiDung).AsNoTracking().AsQueryable();

            if (query.NganhId.HasValue)
            {
                int ngId = query.NganhId.Value;
                baseQuery = baseQuery.Where(g => _db.GiangVienLops
                    .Include(gl => gl.LopHoc)
                    .Any(gl => gl.GiangVienId == g.GiangVienId && gl.LopHoc.NganhId == ngId));
            }
            if (query.KhoaId.HasValue)
                baseQuery = baseQuery.Where(g => g.KhoaId == query.KhoaId);

            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var kw = query.Q.Trim();
                baseQuery = baseQuery.Where(g =>
                    (g.Ms ?? "").Contains(kw) ||
                    (_db.NguoiDungs.Where(nd => nd.NguoiDungId == g.NguoiDungId).Select(nd => nd.HoTen).FirstOrDefault() ?? "").Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(query.MonHoc))
            {
                var mh = query.MonHoc.Trim();
                baseQuery = baseQuery.Where(g => _db.GiangVienLops
                    .Include(gl => gl.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                    .Any(gl => gl.GiangVienId == g.GiangVienId && gl.LopHoc.MonHoc.TenMon.Contains(mh)));
            }

            if (!string.IsNullOrWhiteSpace(query.TrangThai))
            {
                var st = query.TrangThai.ToLower();
                baseQuery = baseQuery.Where(g => _db.NguoiDungs.Any(nd => nd.NguoiDungId == g.NguoiDungId &&
                    ((st == "hoat-dong" && nd.TrangThai == 1) || (st == "khong-hoat-dong" && nd.TrangThai != 1))));
            }

            var baseList = await baseQuery
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new
                {
                    id = g.GiangVienId,
                    nguoiDungId = g.NguoiDungId,
                    ten = g.NguoiDung != null ? g.NguoiDung.HoTen : null
                })
                .ToListAsync();

            var gvIds = baseList.Select(x => x.id).ToList();

            var nganhItems = await (
                from gl in _db.GiangVienLops
                join l in _db.LopHocs on gl.LopHocId equals l.LopHocId
                join ng in _db.Nganhs on l.NganhId equals ng.NganhId into gng
                from ng in gng.DefaultIfEmpty()
                where gvIds.Contains(gl.GiangVienId)
                select new { gl.GiangVienId, TenNganh = ng != null ? ng.TenNganh : null }
            ).Distinct().ToListAsync();
            var nganhMap = nganhItems
                .GroupBy(x => x.GiangVienId)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.TenNganh).Where(s => !string.IsNullOrEmpty(s)).Distinct()));

            var monItems = await (
                from gl in _db.GiangVienLops
                join l in _db.LopHocs on gl.LopHocId equals l.LopHocId
                join m in _db.MonHocs on l.MonHocId equals m.MonHocId
                where gvIds.Contains(gl.GiangVienId)
                select new { gl.GiangVienId, TenMon = m.TenMon }
            ).Distinct().ToListAsync();
            var monMap = monItems
                .GroupBy(x => x.GiangVienId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.TenMon).Distinct().ToList());

            var countMap = await _db.GiangVienLops
                .Where(gl => gvIds.Contains(gl.GiangVienId))
                .GroupBy(gl => gl.GiangVienId)
                .Select(g => new { GiangVienId = g.Key, Count = g.Count() })
                .ToListAsync();
            var countDict = countMap.ToDictionary(x => x.GiangVienId, x => x.Count);

            var ndIds = baseList.Select(x => x.nguoiDungId).Distinct().ToList();
            var ndStatusMap = await _db.NguoiDungs
                .Where(nd => ndIds.Contains(nd.NguoiDungId))
                .Select(nd => new { nd.NguoiDungId, nd.TrangThai })
                .ToListAsync();
            var statusDict = ndStatusMap.ToDictionary(x => x.NguoiDungId, x => x.TrangThai == 1 ? "Hoạt động" : "Không hoạt động");

            var result = baseList.Select(g => new
            {
                id = g.id,
                ten = g.ten,
                nganh = nganhMap.TryGetValue(g.id, out var ngh) ? ngh : string.Empty,
                monPhuTrach = monMap.TryGetValue(g.id, out var mons) ? mons : new List<string>(),
                soLopDangDay = countDict.TryGetValue(g.id, out var cnt) ? cnt : 0,
                trangThai = statusDict.TryGetValue(g.nguoiDungId, out var st) ? st : "Không hoạt động"
            }).ToList();

            return Ok(result);
        }

        [HttpGet("giang-vien/{id:int}")]
        public async Task<IActionResult> GetLecturerDetail(int id)
        {
            var gv = await _db.GiangViens
                .Include(g => g.NguoiDung)
                .FirstOrDefaultAsync(g => g.GiangVienId == id);
            if (gv == null) return NotFound(new { message = "Không tìm thấy giảng viên" });

            var totalClasses = await _db.GiangVienLops.CountAsync(gl => gl.GiangVienId == id);
            var khoa = await _db.Khoas.FirstOrDefaultAsync(k => k.KhoaId == gv.KhoaId);

            var result = new
            {
                maGv = gv.Ms,
                ten = gv.NguoiDung?.HoTen,
                chuyenMon = gv.ChuyenMon,
                hocVi = gv.HocVi,
                chucVu = gv.ChucVu,
                email = gv.NguoiDung?.Email,
                sdt = gv.NguoiDung?.SoDienThoai,
                gioiTinh = gv.NguoiDung?.GioiTinh,
                diaChi = gv.NguoiDung?.DiaChi,
                ngaySinh = gv.NguoiDung?.NgaySinh,
                tongLopPhuTrach = totalClasses,
                khoa = khoa?.TenKhoa
            };

            return Ok(result);
        }

        [HttpGet("giang-vien/{id:int}/lich-day")]
        public async Task<IActionResult> GetLecturerSchedule(int id, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var exists = await _db.GiangViens.AnyAsync(g => g.GiangVienId == id);
            if (!exists) return NotFound(new { message = "Không tìm thấy giảng viên" });

            var classIds = await _db.GiangVienLops
                .Where(gl => gl.GiangVienId == id)
                .Select(gl => gl.LopHocId)
                .ToListAsync();

            var q = _db.BuoiHocs
                .Include(b => b.LopHoc)
                .ThenInclude(l => l.MonHoc)
                .Include(b => b.PhongHoc)
                .Where(b => classIds.Contains(b.LopHocId))
                .AsNoTracking()
                .AsQueryable();

            if (from.HasValue)
                q = q.Where(b => b.ThoiGianBatDau >= from.Value);
            if (to.HasValue)
                q = q.Where(b => b.ThoiGianKetThuc <= to.Value);

            var data = await q.Select(b => new
            {
                buoiHocId = b.BuoiHocId,
                lopHocId = b.LopHocId,
                tenLop = b.LopHoc.TenLop,
                mon = b.LopHoc.MonHoc.TenMon,
                monHocId = b.LopHoc.MonHocId,
                monHocTen = b.LopHoc.MonHoc.TenMon,
                ngay = b.ThoiGianBatDau,
                gioBatDau = b.ThoiGianBatDau,
                gioKetThuc = b.ThoiGianKetThuc,
                phong = b.PhongHoc.TenPhong,
                phongHocId = b.PhongHocId,
                phongHocTen = b.PhongHoc != null ? b.PhongHoc.TenPhong : null,
                diaDiem = b.PhongHoc.DiaChi,
                trangThai = b.TrangThai
            }).OrderBy(x => x.ngay).ToListAsync();

            return Ok(data);
        }

        [HttpPost("lich-day")]
        public async Task<IActionResult> CreateBuoiHoc([FromBody] TruongKhoaCreateBuoiHocRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var lop = await _db.LopHocs.Include(l => l.MonHoc).FirstOrDefaultAsync(l => l.LopHocId == req.LopHocId);
            if (lop == null) return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });
            var phong = await _db.PhongHocs.FirstOrDefaultAsync(p => p.PhongHocId == req.PhongHocId);
            if (phong == null) return BadRequest(new { field = "phongHocId", message = "Phòng học không tồn tại" });

            int? giangVienId = req.GiangVienId;
            if (!giangVienId.HasValue)
            {
                giangVienId = await _db.GiangVienLops.Where(gl => gl.LopHocId == req.LopHocId)
                    .Select(gl => gl.GiangVienId).FirstOrDefaultAsync();
            }

            if (giangVienId.HasValue)
            {
                var assigned = await _db.GiangVienLops.AnyAsync(gl => gl.LopHocId == req.LopHocId && gl.GiangVienId == giangVienId.Value);
                if (!assigned)
                    return BadRequest(new { field = "giangVienId", message = "Giảng viên không phụ trách lớp này" });
            }

            var conflictRoom = await _db.BuoiHocs.AnyAsync(b => b.PhongHocId == req.PhongHocId &&
                b.ThoiGianBatDau < req.ThoiGianKetThuc && req.ThoiGianBatDau < b.ThoiGianKetThuc);

            bool conflictTeacher = false;
            if (giangVienId.HasValue)
            {
                var gvClassIds = await _db.GiangVienLops.Where(gl => gl.GiangVienId == giangVienId.Value)
                    .Select(gl => gl.LopHocId).Distinct().ToListAsync();
                conflictTeacher = await _db.BuoiHocs.AnyAsync(b => gvClassIds.Contains(b.LopHocId) &&
                    b.ThoiGianBatDau < req.ThoiGianKetThuc && req.ThoiGianBatDau < b.ThoiGianKetThuc);
            }

            if ((conflictRoom || conflictTeacher) && !req.OverrideConflicts)
            {
                return Conflict(new
                {
                    message = "Xung đột lịch",
                    conflictRoom,
                    conflictTeacher
                });
            }

            var entity = new BuoiHoc
            {
                LopHocId = req.LopHocId,
                PhongHocId = req.PhongHocId,
                ThoiGianBatDau = req.ThoiGianBatDau,
                ThoiGianKetThuc = req.ThoiGianKetThuc,
                GhiChu = req.GhiChu,
                LoaiBuoiHoc = "giang bai",
                CreatedAt = DateTime.UtcNow
            };

            _db.BuoiHocs.Add(entity);
            await _db.SaveChangesAsync();

            if ((conflictRoom || conflictTeacher) && req.OverrideConflicts)
            {
                _db.NhatKyHeThongs.Add(new NhatKyHeThong
                {
                    NguoiDungId = req.ActorNguoiDungId,
                    HanhDong = "OverrideBuoiHoc",
                    ThamChieu = $"BuoiHocId={entity.BuoiHocId}; reason={req.OverrideReason}; roomConflict={conflictRoom}; teacherConflict={conflictTeacher}",
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetLecturerSchedule), new { id = giangVienId ?? 0 }, new { buoiHocId = entity.BuoiHocId });
        }

        [HttpPut("lich-day/{id:int}")]
        public async Task<IActionResult> UpdateBuoiHoc(int id, [FromBody] TruongKhoaUpdateBuoiHocRequest req)
        {
            var entity = await _db.BuoiHocs.FirstOrDefaultAsync(b => b.BuoiHocId == id);
            if (entity == null) return NotFound(new { message = "Không tìm thấy buổi học" });

            var conflictRoom = await _db.BuoiHocs.AnyAsync(b => b.BuoiHocId != id && b.PhongHocId == req.PhongHocId &&
                b.ThoiGianBatDau < req.ThoiGianKetThuc && req.ThoiGianBatDau < b.ThoiGianKetThuc);

            var gvClassIds = await _db.GiangVienLops.Where(gl => gl.LopHocId == entity.LopHocId)
                .Select(gl => gl.GiangVienId).ToListAsync();
            bool conflictTeacher = false;
            if (gvClassIds.Count > 0)
            {
                var teacherClassIds = await _db.GiangVienLops.Where(gl => gvClassIds.Contains(gl.GiangVienId))
                    .Select(gl => gl.LopHocId).Distinct().ToListAsync();
                conflictTeacher = await _db.BuoiHocs.AnyAsync(b => b.BuoiHocId != id && teacherClassIds.Contains(b.LopHocId) &&
                    b.ThoiGianBatDau < req.ThoiGianKetThuc && req.ThoiGianBatDau < b.ThoiGianKetThuc);
            }

            if ((conflictRoom || conflictTeacher) && !req.OverrideConflicts)
            {
                return Conflict(new
                {
                    message = "Xung đột lịch",
                    conflictRoom,
                    conflictTeacher
                });
            }

            entity.PhongHocId = req.PhongHocId;
            entity.ThoiGianBatDau = req.ThoiGianBatDau;
            entity.ThoiGianKetThuc = req.ThoiGianKetThuc;
            entity.GhiChu = req.GhiChu;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            if ((conflictRoom || conflictTeacher) && req.OverrideConflicts)
            {
                _db.NhatKyHeThongs.Add(new NhatKyHeThong
                {
                    NguoiDungId = req.ActorNguoiDungId,
                    HanhDong = "OverrideBuoiHocUpdate",
                    ThamChieu = $"BuoiHocId={entity.BuoiHocId}; reason={req.OverrideReason}; roomConflict={conflictRoom}; teacherConflict={conflictTeacher}",
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpPost("lich-thi")]
        public async Task<IActionResult> CreateBuoiThi([FromBody] TruongKhoaCreateBuoiThiRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var lop = await _db.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == req.LopHocId);
            if (lop == null) return BadRequest(new { field = "lopHocId", message = "Lớp học không tồn tại" });
            var phong = await _db.PhongHocs.FirstOrDefaultAsync(p => p.PhongHocId == req.PhongHocId);
            if (phong == null) return BadRequest(new { field = "phongHocId", message = "Phòng học không tồn tại" });

            var conflictRoom = await _db.BuoiThis.AnyAsync(b => b.PhongHocId == req.PhongHocId &&
                EF.Functions.DateDiffDay(b.NgayThi, req.NgayThi) == 0);

            bool conflictTeacher = false;
            if (req.GiamThiId.HasValue)
            {
                var gvClassIds = await _db.GiangVienLops.Where(gl => gl.GiangVienId == req.GiamThiId.Value)
                    .Select(gl => gl.LopHocId).Distinct().ToListAsync();
                conflictTeacher = await _db.BuoiHocs.AnyAsync(b => gvClassIds.Contains(b.LopHocId) &&
                    EF.Functions.DateDiffDay(b.ThoiGianBatDau, req.NgayThi) >= 0 &&
                    EF.Functions.DateDiffDay(req.NgayThi, b.ThoiGianKetThuc) >= 0);
            }

            if ((conflictRoom || conflictTeacher) && !req.OverrideConflicts)
            {
                return Conflict(new
                {
                    message = "Xung đột lịch",
                    conflictRoom,
                    conflictTeacher
                });
            }

            var entity = new BuoiThi
            {
                LopHocId = req.LopHocId,
                PhongHocId = req.PhongHocId,
                NgayThi = req.NgayThi,
                HinhThuc = req.HinhThuc,
                GiamThiId = req.GiamThiId,
                CreatedAt = DateTime.UtcNow
            };
            _db.BuoiThis.Add(entity);
            await _db.SaveChangesAsync();

            if ((conflictRoom || conflictTeacher) && req.OverrideConflicts)
            {
                _db.NhatKyHeThongs.Add(new NhatKyHeThong
                {
                    NguoiDungId = req.ActorNguoiDungId,
                    HanhDong = "OverrideBuoiThi",
                    ThamChieu = $"BuoiThiId={entity.BuoiThiId}; reason={req.OverrideReason}; roomConflict={conflictRoom}; teacherConflict={conflictTeacher}",
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(CreateBuoiThi), new { id = entity.BuoiThiId }, new { buoiThiId = entity.BuoiThiId });
        }

        public class TruongKhoaCreateThongBaoRequest
        {
            [Required]
            public string TieuDe { get; set; } = string.Empty;
            public string? NoiDung { get; set; }
            [Required]
            public int NguoiDangId { get; set; }
            public string Target { get; set; } = "all"; // "toan-khoa"|"nganh"|"khoa-tuyen-sinh"|"mon"|"lop"|"giang-vien"|"sv"
            public int? GroupId { get; set; }
        }

        [HttpPost("thong-bao")]
        public async Task<IActionResult> CreateThongBao([FromBody] TruongKhoaCreateThongBaoRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ndExists = await _db.NguoiDungs.AnyAsync(x => x.NguoiDungId == req.NguoiDangId);
            if (!ndExists) return BadRequest(new { field = "nguoiDangId", message = "Người gửi không tồn tại" });

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

            var nguoiDangTen = await _db.NguoiDungs
                .Where(nd => nd.NguoiDungId == tb.NguoiDangId)
                .Select(nd => nd.HoTen)
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetDashboardSummary), new { id = tb.ThongBaoId }, new { id = tb.ThongBaoId, tieuDe = tb.TieuDe, nguoiDangTen });
        }

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
                var ndId = await _db.GiangViens.Where(g => g.GiangVienId == gvId)
                    .Select(g => g.NguoiDungId).FirstOrDefaultAsync();
                if (ndId != 0) result.Add(ndId);
            }
            else if (target == "sv" && groupId.HasValue)
            {
                int svId = groupId.Value;
                var ndId = await _db.HoSoSinhViens.Where(s => s.SinhVienId == svId)
                    .Select(s => s.NguoiDungId).FirstOrDefaultAsync();
                if (ndId != 0) result.Add(ndId);
            }

            return result.Distinct().ToList();
        }
    }
}
