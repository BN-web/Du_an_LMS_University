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
using System;
using System.Globalization;


namespace LMS_GV.Controllers_GiangVien
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Giảng Viên")]
    public class GV_TaiLieuBaiTapController : Controller
    {
        private readonly AppDbContext _context;

        public GV_TaiLieuBaiTapController(AppDbContext context)
        {
            _context = context;
        }
        ///----------------Trang 1: Trang bài tập  ----------------///

        /// <summary>
        /// Lấy tất cả bài tập/kiểm tra/bài thi của giảng viên (tất cả lớp)
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giang-vien/bai-tap/danh-sach")]
        public async Task<ActionResult<List<BaiTapGiangVienDto>>> GetDanhSachBaiTapGiangVien()
        {
            // Lấy GiangVien_id từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Query lấy toàn bộ bài kiểm tra của giảng viên
            var result = await _context.BaiKiemTras
                .Where(b => b.GiangVienId == giangVienId)
                .Select(b => new BaiTapGiangVienDto
                {
                    BaiKiemTraId = b.BaiKiemTraId,
                    TieuDe = b.TieuDe,

                    NgayTao = b.CreatedAt.HasValue
                    ? b.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm")
                    : "",
                    HanNop = b.NgayKetThuc.HasValue
                    ? b.NgayKetThuc.Value.ToString("dd/MM/yyyy HH:mm")
                    : "",
                    Loai = b.Loai,

                    TenMon = b.MonHoc != null ? b.MonHoc.TenMon : "",
                    MaLop = b.LopHoc != null ? b.LopHoc.MaLop : "",

                    TongSinhVien = b.LopHoc.SinhVienLops.Count(),
                    DaNop = b.BaiNops.Count(),
                    SoLanLamBai = b.BaiNops.Count()
                })
                .ToListAsync();

            foreach (var x in result)
            {
                x.ChuaNop = x.TongSinhVien - x.DaNop;
                x.TienDoPhanTram = x.TongSinhVien == 0
                    ? 0
                    : Math.Round((double)x.DaNop / x.TongSinhVien * 100, 2);
            }

            return Ok(result);
        }


        /// <summary> 
        /// API search tìm kiếm bài tập theo: mã lớp, tên môn, tiêu đề bài kiểm tra
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giang-vien/bai-tap/search-baitap")]
        public async Task<ActionResult<List<BaiTapGiangVienDto>>> SearchBaiTapGiangVien(
          [FromQuery] string? maLop,
          [FromQuery] string? tenMon,
          [FromQuery] string? tieuDe)
        {
            // Lấy giảng viên từ token
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var query = _context.BaiKiemTras
                .Where(bt => bt.GiangVienId == giangVienId)
                .AsQueryable();

            // filter
            if (!string.IsNullOrEmpty(maLop))
                query = query.Where(bt => bt.LopHoc.MaLop.Contains(maLop));

            if (!string.IsNullOrEmpty(tenMon))
                query = query.Where(bt => bt.MonHoc.TenMon.Contains(tenMon));

            if (!string.IsNullOrEmpty(tieuDe))
                query = query.Where(bt => bt.TieuDe.Contains(tieuDe));

            var data = await query
                .Select(bt => new BaiTapGiangVienDto
                {
                    BaiKiemTraId = bt.BaiKiemTraId,
                    TieuDe = bt.TieuDe,
                    NgayTao = bt.CreatedAt.HasValue
                    ? bt.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm")
                    : "",
                    HanNop = bt.NgayKetThuc.HasValue
                    ? bt.NgayKetThuc.Value.ToString("dd/MM/yyyy HH:mm")
                    : "",
                    Loai = bt.Loai,
                    TenMon = bt.MonHoc.TenMon,
                    MaLop = bt.LopHoc.MaLop,
                    TongSinhVien = bt.LopHoc.SinhVienLops.Count(),
                    DaNop = bt.BaiNops.Count(),
                    SoLanLamBai = bt.BaiNops.Sum(bn => bn.LanLam ?? 1) // tổng số lần làm của tất cả sinh viên
                })
                .ToListAsync();

            // tính toán tiến độ
            foreach (var x in data)
            {
                x.ChuaNop = x.TongSinhVien - x.DaNop;
                x.TienDoPhanTram = x.TongSinhVien == 0 ? 0 : Math.Round((double)x.DaNop / x.TongSinhVien * 100, 2);
            }

            return Ok(data);
        }



        ///------------Tạo bài tập-----------------///

        [Authorize(Roles = "Giảng Viên")]
        [HttpPost("giang-vien/bai-tap/tao-moi")]
        public async Task<ActionResult> TaoBaiTap([FromBody] TaoBaiTapRequestDto dto)
        {
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var hanNopDate = !string.IsNullOrEmpty(dto.HanNop)
     ? DateTime.ParseExact(dto.HanNop, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
     : (DateTime?)null;

            var bai = new BaiKiemTra
            {
                TieuDe = dto.TieuDe,
                Loai = dto.Loai,
                NgayBatDau = DateTime.Now,
                NgayKetThuc = hanNopDate,
                SoLanLamToiDa = dto.SoLanLamToiDa,
                ThoiGianLamBai = dto.ThoiGianLamBai,
                GiangVienId = giangVienId,
                LopHocId = dto.LopHocId,
                CreatedAt = DateTime.Now
            };


            _context.BaiKiemTras.Add(bai);
            await _context.SaveChangesAsync();

            foreach (var ch in dto.CauHois)
            {
                var cauHoi = new CauHoi
                {
                    BaiKiemTraId = bai.BaiKiemTraId,
                    NoiDung = ch.NoiDung
                };

                _context.CauHois.Add(cauHoi);
                await _context.SaveChangesAsync();

                for (int i = 0; i < ch.DapAns.Count; i++)
                {
                    var da = ch.DapAns[i];
                    _context.TuyChonCauHois.Add(new TuyChonCauHoi
                    {
                        CauHoiId = cauHoi.CauHoiId,
                        MaLuaChon = ((char)('A' + i)).ToString(),
                        NoiDung = da.NoiDung,
                        LaDapAn = (i == ch.DapAnDung),
                        ThuTu = da.ThuTu
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo bài tập thành công",
                BaiKiemTraId = bai.BaiKiemTraId
            });
        }


        // API: thay đổi thứ tự dáp án trong câu hỏi
        [Authorize(Roles = "Giảng Viên")]
        [HttpPut("giang-vien/cau-hoi/thay-doi-thu-tu")]
        public async Task<ActionResult> ThayDoiThuTuDapAn([FromBody] ThayDoiThuTuDapAnDto dto)
        {
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Kiểm tra câu hỏi có thuộc giảng viên
            var cauHoi = await _context.CauHois
                .Include(ch => ch.BaiKiemTra)
                .FirstOrDefaultAsync(ch => ch.CauHoiId == dto.CauHoiId
                                           && ch.BaiKiemTra.GiangVienId == giangVienId);

            if (cauHoi == null)
                return NotFound("Câu hỏi không tồn tại hoặc không thuộc giảng viên.");

            foreach (var item in dto.ThuTus)
            {
                var dapAn = await _context.TuyChonCauHois
                    .FirstOrDefaultAsync(t => t.TuyChonCauHoiId == item.TuyChonCauHoiId && t.CauHoiId == dto.CauHoiId);

                if (dapAn != null)
                {
                    dapAn.ThuTu = item.ThuTuMoi;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thứ tự đáp án thành công" });
        }


        ///------------Xem chi tiết bài tập/bài kiểm tra/bài thi-----------------///

        //
        //API 1: Xem thông tin tổng quan bài kiểm tra theo lớp học của giảng viên (token)

        //GET: /giang-vien/bai-kiem-tra/chi-tiet/{id}
        //Trả về:

        //tiêu đề

        //mô tả

        //loại bài

        //tên môn học

        //deadline định dạng: HH:mm dd/MM/yyyy

        //thời gian làm bài

        //tổng số sinh viên đã nộp

        // số câu hỏi
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giang-vien/bai-kiem-tra/chi-tiet/{id}-Bai-kiem-tra/Tong-quan")]
        public async Task<ActionResult> ChiTietBaiKiemTra(int id)
        {
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var bai = await _context.BaiKiemTras
                .Include(x => x.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                .FirstOrDefaultAsync(x => x.BaiKiemTraId == id && x.GiangVienId == giangVienId);

            if (bai == null)
                return NotFound("Bài kiểm tra không tồn tại hoặc không thuộc giảng viên.");

            var tongDaNop = await _context.BaiNops
                .CountAsync(x => x.BaiKiemTraId == id && x.TrangThai == 1);

            var tongCauHoi = await _context.CauHois
                .CountAsync(x => x.BaiKiemTraId == id);

            var dto = new BaiKiemTraChiTietDto
            {
                BaiKiemTraId = bai.BaiKiemTraId,
                TieuDe = bai.TieuDe,
                MoTa = bai.MoTa,
                Loai = bai.Loai,
                TenMonHoc = bai.LopHoc?.MonHoc?.TenMon,
                HanNop = bai.NgayKetThuc?.ToString("HH:mm dd/MM/yyyy"),
                ThoiGianLamBai = bai.ThoiGianLamBai,
                TongDaNop = tongDaNop,
                TongCauHoi = tongCauHoi,
                SoLanLamToiDa = bai.SoLanLamToiDa 
            };

            return Ok(dto);
        }


        //Danh sách sinh viên đã nộp bài
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giang-vien/bai-kiem-tra/danh-sach-nop/{id}")]
        public async Task<ActionResult> DanhSachSinhVienNop(int id)
        {
            // Lấy giảng viên từ token
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Kiểm tra bài kiểm tra có thuộc giảng viên không
            var bai = await _context.BaiKiemTras
                .FirstOrDefaultAsync(x => x.BaiKiemTraId == id && x.GiangVienId == giangVienId);

            if (bai == null)
                return NotFound("Bài kiểm tra không tồn tại hoặc không thuộc giảng viên.");

            // Lấy danh sách sinh viên đã nộp bài
            var ds = await _context.BaiNops
                .Include(x => x.SinhVien)
                    .ThenInclude(s => s.NguoiDung)
                .Include(x => x.BaiKiemTra) // include để lấy SoLanLamToiDa
                .Where(x => x.BaiKiemTraId == id)
                .Select(x => new SinhVienNopBaiDto
                {
                    MSSV = x.SinhVien.Mssv,
                    HoTen = x.SinhVien.NguoiDung.HoTen,
                    ThoiGianNop = x.NgayNop.HasValue
                        ? x.NgayNop.Value.ToString("HH:mm dd/MM/yyyy")
                        : "",
                    LanLam = x.LanLam ?? 1, 
                    TrangThai = x.TrangThai == 1 ? "Đã chấm" : "Gian lận",
                    Diem = x.TongDiem
                })
                .ToListAsync();

            return Ok(ds);
        }

        /// <summary>
        /// Tổng quan bài làm của sinh viên trong lớp theo bài kiểm tra
        /// </summary>
        [HttpGet("giang-vien/bai-kiem-tra/{baiKiemTraId}/tong-quan-sinh-vien")]
        public async Task<ActionResult<List<BaiLamSinhVienDto>>> TongQuanBaiLam(int baiKiemTraId)
        {
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Lấy bài kiểm tra, kiểm tra quyền
            var bai = await _context.BaiKiemTras
                .Include(x => x.LopHoc)
                .FirstOrDefaultAsync(x => x.BaiKiemTraId == baiKiemTraId && x.GiangVienId == giangVienId);

            if (bai == null)
                return NotFound("Bài kiểm tra không tồn tại hoặc không thuộc giảng viên.");

            // Lấy danh sách bài nộp của sinh viên
            var ds = await _context.BaiNops
                .Include(x => x.SinhVien)
                    .ThenInclude(s => s.NguoiDung)
                .Include(x => x.ChiTietCauTraLois)
                .Where(x => x.BaiKiemTraId == baiKiemTraId)
                .Select(x => new BaiLamSinhVienDto
                {
                    MSSV = x.SinhVien.Mssv,
                    HoTen = x.SinhVien.NguoiDung.HoTen,
                    HanNop = bai.NgayKetThuc,
                    TongDiemBaiKiemTra = bai.DiemToiDa,
                    TongSoCauHoi = bai.SoCau ?? 0,
                    TongCauDung = x.ChiTietCauTraLois.Count(c => c.Diem > 0),
                    PhanTramDung = (bai.SoCau ?? 0) > 0 ?
                        Math.Round((double)x.ChiTietCauTraLois.Count(c => c.Diem > 0) / (bai.SoCau ?? 0) * 100, 2) : 0
                })
                .ToListAsync();

            return Ok(ds);
        }

        //Get: hiện các câu hỏi của bài kiểm tra
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giang-vien/bai-kiem-tra/{baiKiemTraId}/cau-hoi")]
        public async Task<ActionResult<BaiKiemTraCauHoiDto>> GetCauHoiBaiKiemTra(int baiKiemTraId)
        {
            // Lấy GiangVien_id từ token
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Kiểm tra bài kiểm tra thuộc giảng viên
            var baiKiemTra = await _context.BaiKiemTras
                .Include(bt => bt.CauHois)
                    .ThenInclude(ch => ch.TuyChonCauHois)
                .FirstOrDefaultAsync(bt => bt.BaiKiemTraId == baiKiemTraId && bt.GiangVienId == giangVienId);

            if (baiKiemTra == null)
                return NotFound("Bài kiểm tra không tồn tại hoặc không thuộc giảng viên.");

            var dto = new BaiKiemTraCauHoiDto
            {
                BaiKiemTraId = baiKiemTra.BaiKiemTraId,
                TieuDe = baiKiemTra.TieuDe,
                TongSoCauHoi = baiKiemTra.CauHois.Count,
                CauHois = baiKiemTra.CauHois
                    .OrderBy(ch => ch.CauHoiId) // hoặc theo ThuTu nếu có
                    .Select(ch => new CauHoiChiTietDto
                    {
                        CauHoiId = ch.CauHoiId,
                        NoiDung = ch.NoiDung,
                        Diem = ch.Diem,
                        DapAns = ch.TuyChonCauHois
                            .OrderBy(t => t.ThuTu ?? 0)
                            .Select(t => new DapAnDto
                            {
                                MaLuaChon = t.MaLuaChon,
                                NoiDung = t.NoiDung,
                                LaDapAn = t.LaDapAn
                            }).ToList()
                    }).ToList()
            };

            return Ok(dto);
        }


        /// <summary>
        /// Chi tiết các câu hỏi và đáp án của sinh viên theo bài kiểm tra
        /// </summary>
        [HttpGet("giang-vien/bai-kiem-tra/{baiNopId}/chi-tiet-cau-hoi")]
        public async Task<ActionResult<List<ChiTietCauHoiSinhVienDto>>> ChiTietCauHoiBaiLam(int baiNopId)
        {
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var baiNop = await _context.BaiNops
                .Include(x => x.BaiKiemTra)
                .Include(x => x.SinhVien)
                .Include(x => x.ChiTietCauTraLois)
                    .ThenInclude(c => c.CauHoi)
                        .ThenInclude(ch => ch.TuyChonCauHois)
                .FirstOrDefaultAsync(x => x.BaiNopId == baiNopId && x.BaiKiemTra.GiangVienId == giangVienId);

            if (baiNop == null)
                return NotFound("Không tìm thấy bài nộp hoặc không thuộc quyền giảng viên.");

            var ds = baiNop.ChiTietCauTraLois.Select(c => new ChiTietCauHoiSinhVienDto
            {
                NoiDungCauHoi = c.CauHoi.NoiDung,
                DiemSinhVien = c.Diem,
                DiemToiDa = c.CauHoi.Diem,
                MaLuaChon = c.LuaChonSinhVien,
                NoiDungLuaChon = c.LuaChonSinhVien != null ?
                                 c.CauHoi.TuyChonCauHois.FirstOrDefault(t => t.MaLuaChon == c.LuaChonSinhVien)?.NoiDung
                                 : ""
            }).ToList();

            return Ok(ds);
        }

        ///------------Trang tài liệu-----------------///

        //API: hiện danh sách tài liệu 
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giang-vien/danh-sach-tai-lieu")]
        public async Task<IActionResult> DanhSachTaiLieu()
        {
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var ds = await _context.BaiHocLops
                .Include(bhl => bhl.BaiHoc)
                    .ThenInclude(b => b.Files)
                .Include(bhl => bhl.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                .Where(bhl => bhl.LopHoc.GiangVienId == giangVienId)
                .SelectMany(bhl => bhl.BaiHoc.Files, (bhl, f) => new TaiLieuDto
                {
                    TenMonHoc = bhl.LopHoc.MonHoc.TenMon,
                    TongTaiLieuLop = bhl.BaiHoc.Files.Count(),
                    TenBaiHoc = bhl.BaiHoc.TieuDe,
                    LoaiBaiHoc = bhl.BaiHoc.LoaiBaiHoc,
                    NgayUpload = f.CreatedAt.HasValue ? f.CreatedAt.Value.ToString("dd/MM/yyyy") : "",
                    KichThuoc = f.KichThuoc ?? 0
                })
                .ToListAsync();

            return Ok(ds);
        }

        /// <summary> 
        /// API search tìm kiếm tài liệu theo: tên môn, tên tài liệu
        /// </summary>
        [Authorize(Roles = "Giảng Viên")]
        [HttpGet("giang-vien/tai-lieu/search-tai-lieu")]
        public async Task<ActionResult<List<TaiLieuDto>>> SearchTaiLieuGiangVien(
    [FromQuery] string? tenMon,
    [FromQuery] string? tenBaiHoc)
        {
            var giangVienClaim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(giangVienClaim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            var query = _context.BaiHocLops
                .Include(bhl => bhl.BaiHoc)
                    .ThenInclude(bh => bh.Files)
                .Include(bhl => bhl.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                .Where(bhl => bhl.BaiHoc.CreatedBy == giangVienId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tenMon))
                query = query.Where(bhl => bhl.LopHoc.MonHoc.TenMon.Contains(tenMon));

            if (!string.IsNullOrEmpty(tenBaiHoc))
                query = query.Where(bhl => bhl.BaiHoc.TieuDe.Contains(tenBaiHoc));

            var data = await query
                .Select(bhl => new TaiLieuDto
                {
                    TenMonHoc = bhl.LopHoc.MonHoc.TenMon,
                    TongTaiLieuLop = bhl.BaiHoc.Files.Count(),
                    TenBaiHoc = bhl.BaiHoc.TieuDe,
                    LoaiBaiHoc = bhl.BaiHoc.LoaiBaiHoc,
                    NgayUpload = bhl.BaiHoc.CreatedAt.HasValue
                        ? bhl.BaiHoc.CreatedAt.Value.ToString("dd/MM/yyyy")
                        : "",
                    KichThuoc = bhl.BaiHoc.Files.Sum(f => f.KichThuoc ?? 0)
                })
                .ToListAsync();

            return Ok(data);
        }

        ///------------Tải tài liệu-----------------///

        //Api: Upload file 
        [Authorize(Roles = "Giảng Viên")]
        [HttpPost("giang-vien/tai-lieu/upload")]
        public async Task<IActionResult> UploadTaiLieu([FromForm] UploadTaiLieuDto dto)
        {
            var claim = User.FindFirst("GiangVien_id")?.Value;
            if (!int.TryParse(claim, out int giangVienId))
                return Unauthorized("GiangVien_id không hợp lệ");

            // Kiểm tra lớp thuộc giảng viên
            var lopHoc = await _context.LopHocs
                .FirstOrDefaultAsync(l => l.LopHocId == dto.LopHocId && l.GiangVienId == giangVienId);
            if (lopHoc == null)
                return Unauthorized("Bạn không có quyền đăng tài liệu cho lớp này.");

            // Kiểm tra bài học
            var baiHoc = await _context.BaiHocs.FirstOrDefaultAsync(b => b.BaiHocId == dto.BaiHocId);
            if (baiHoc == null)
                return NotFound("Bài học không tồn tại.");

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "files");
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var fileEntity = new LMS_GV.Models.File
            {
                BaiHocId = dto.BaiHocId,
                TenFile = dto.File.FileName,
                DuongDan = $"/uploads/files/{fileName}",
                KichThuoc = dto.File.Length,
                CreatedAt = DateTime.Now
            };

            _context.Files.Add(fileEntity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Upload thành công",
                DuongDan = fileEntity.DuongDan
            });
        }



        //API Tải tài liệu dựa theo thuộc tính trong danh sách tài liệu 
        [Authorize(Roles = "Giảng Viên")]
        [HttpPost("giang-vien/tai-lieu/tai-ve")]
        public async Task<IActionResult> TaiTaiLieuVe([FromBody] TaiFileRequestDto dto)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.FilesId == dto.FileId);
            if (file == null)
                return NotFound("File không tồn tại");

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.DuongDan.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound("File không tồn tại trên server");

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", file.TenFile);
        }


    }
}
