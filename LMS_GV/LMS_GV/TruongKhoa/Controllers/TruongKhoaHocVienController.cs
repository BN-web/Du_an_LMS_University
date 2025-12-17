using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using LMS_GV.DTOs.TruongKhoa;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LMS_GV.Controllers.TruongKhoa
{
    [Route("api/truong-bo-mon/hoc-vien")]
    [ApiController]
    [Authorize]
    public class TruongKhoaHocVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TruongKhoaHocVienController> _logger;

        public TruongKhoaHocVienController(AppDbContext context, ILogger<TruongKhoaHocVienController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lấy ID Trưởng khoa từ token
        private int GetTruongKhoaId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng");
        }

        // Lấy Khoa của Trưởng khoa
        private async Task<int?> GetKhoaIdOfTruongKhoa()
        {
            var truongKhoaId = GetTruongKhoaId();
            var giangVien = await _context.GiangViens
                .FirstOrDefaultAsync(gv => gv.NguoiDungId == truongKhoaId && gv.ChucVu == "Trưởng khoa");

            return giangVien?.KhoaId;
        }

        // 1. THỐNG KÊ TỔNG QUAN
        [HttpGet("thong-ke")]
        public async Task<ActionResult<TruongKhoaApiResponse<ThongKeHocVienResponseDTO>>> GetThongKe()
        {
            try
            {
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Lấy tất cả học viên thuộc khoa
                var hocViens = await _context.HoSoSinhViens
                    .Include(sv => sv.Nganh)
                    .Where(sv => sv.Nganh != null && sv.Nganh.KhoaId == khoaIdTruongKhoa.Value)
                    .ToListAsync();

                var tongSoHocVien = hocViens.Count;
                var hocVienHoatDong = hocViens.Count(sv => sv.NguoiDung != null && sv.NguoiDung.TrangThai == 1);
                var hocVienDaNghi = hocViens.Count(sv => sv.NguoiDung != null && sv.NguoiDung.TrangThai == 3);

                // Đếm số lớp học thuộc khoa
                var tongSoLop = await _context.LopHocs
                    .CountAsync(l => l.KhoaId == khoaIdTruongKhoa.Value && l.TrangThai == 1);

                // Đếm số giảng viên thuộc khoa
                var tongGiangVien = await _context.GiangViens
                    .CountAsync(gv => gv.KhoaId == khoaIdTruongKhoa.Value);

                // Lấy thông báo gần đây (5 thông báo mới nhất)
                var thongBaoGanDay = await _context.ThongBaos
                    .OrderByDescending(tb => tb.NgayTao)
                    .Take(5)
                    .Select(tb => new ThongBaoGanDayDTO
                    {
                        NoiDung = tb.NoiDung ?? "",
                        ThoiGian = tb.NgayTao.HasValue ? tb.NgayTao.Value.ToString("dd/MM/yyyy HH:mm") : ""
                    })
                    .ToListAsync();

                // Bảng điểm hoàn thành bài tập (lấy từ các lớp thuộc khoa)
                var lopHocs = await _context.LopHocs
                    .Include(l => l.MonHoc)
                    .Where(l => l.KhoaId == khoaIdTruongKhoa.Value && l.TrangThai == 1)
                    .ToListAsync();

                var bieuDoHoanThanhBaiTap = new List<BieuDoHoanThanhBaiTapDTO>();
                foreach (var lop in lopHocs.Take(4)) // Lấy 4 lớp đầu tiên
                {
                    var baiKiemTras = await _context.BaiKiemTras
                        .Where(bkt => bkt.LopHocId == lop.LopHocId && bkt.Loai == "Bài tập")
                        .ToListAsync();

                    var tongBaiTap = baiKiemTras.Count;
                    var daNop = await _context.BaiNops
                        .CountAsync(bn => baiKiemTras.Select(bkt => bkt.BaiKiemTraId).Contains(bn.BaiKiemTraId));

                    bieuDoHoanThanhBaiTap.Add(new BieuDoHoanThanhBaiTapDTO
                    {
                        MonHoc = lop.MonHoc?.TenMon ?? "",
                        DaNop = daNop,
                        ChuaNop = Math.Max(0, tongBaiTap - daNop)
                    });
                }

                var response = new ThongKeHocVienResponseDTO
                {
                    TongSoHocVien = tongSoHocVien,
                    HocVienHoatDong = hocVienHoatDong,
                    HocVienDaNghi = hocVienDaNghi,
                    TongSoLop = tongSoLop,
                    TongGiangVien = tongGiangVien,
                    ThongBaoGanDay = thongBaoGanDay,
                    BieuDoHoanThanhBaiTap = bieuDoHoanThanhBaiTap
                };

                return Ok(new TruongKhoaApiResponse<ThongKeHocVienResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê học viên");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 2. DANH SÁCH HỌC VIÊN
        [HttpGet]
        public async Task<ActionResult<TruongKhoaPagedResponse<HocVienListDTO>>> GetDanhSachHocVien(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? khoaId = null,
            [FromQuery] int? nganhId = null,
            [FromQuery] int? khoaTuyenSinhId = null,
            [FromQuery] byte? trangThai = null)
        {
            try
            {
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaPagedResponse<HocVienListDTO>
                    {
                        Success = false,
                        Data = new TruongKhoaPagedData<HocVienListDTO> { Items = new List<HocVienListDTO>() }
                    });
                }

                var query = _context.HoSoSinhViens
                    .Include(sv => sv.NguoiDung)
                    .Include(sv => sv.Nganh)
                    .ThenInclude(n => n.Khoa)
                    .Include(sv => sv.KhoaTuyenSinh)
                    .Where(sv => sv.Nganh != null && sv.Nganh.KhoaId == khoaIdTruongKhoa.Value);

                // Áp dụng bộ lọc
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(sv =>
                        (sv.Mssv != null && sv.Mssv.Contains(search)) ||
                        (sv.NguoiDung != null && sv.NguoiDung.HoTen != null && sv.NguoiDung.HoTen.Contains(search)));
                }

                if (nganhId.HasValue)
                {
                    query = query.Where(sv => sv.NganhId == nganhId.Value);
                }

                if (khoaTuyenSinhId.HasValue)
                {
                    query = query.Where(sv => sv.KhoaTuyenSinhId == khoaTuyenSinhId.Value);
                }

                if (trangThai.HasValue)
                {
                    query = query.Where(sv => sv.NguoiDung != null && sv.NguoiDung.TrangThai == trangThai.Value);
                }

                // Phân trang
                var total = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(total / (double)limit);

                var sinhVienIds = await query
                    .OrderByDescending(sv => sv.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(sv => sv.SinhVienId)
                    .ToListAsync();

                // Lấy thông tin học viên với đầy đủ includes
                var hocViensData = await _context.HoSoSinhViens
                    .Include(sv => sv.NguoiDung)
                    .Include(sv => sv.Nganh)
                    .ThenInclude(n => n.Khoa)
                    .Include(sv => sv.KhoaTuyenSinh)
                    .Include(sv => sv.SinhVienLops)
                    .ThenInclude(sl => sl.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                    .Include(sv => sv.SinhVienLops)
                    .ThenInclude(sl => sl.LopHoc)
                    .ThenInclude(l => l.GiangVien)
                    .ThenInclude(gv => gv.NguoiDung)
                    .Where(sv => sinhVienIds.Contains(sv.SinhVienId))
                    .ToListAsync();

                var hocViens = hocViensData.Select(sv =>
                {
                    var lopHocHienTai = sv.SinhVienLops
                        .FirstOrDefault(sl => sl.TinhTrang == "dang hoc")?.LopHoc;

                    return new HocVienListDTO
                    {
                        Id = sv.SinhVienId,
                        Mssv = sv.Mssv,
                        HoTen = sv.NguoiDung?.HoTen,
                        Khoa = sv.KhoaTuyenSinh?.TenKhoaTuyenSinh,
                        TenKhoa = sv.Nganh?.Khoa?.TenKhoa,
                        Nganh = sv.Nganh?.TenNganh,
                        MonLop = lopHocHienTai?.MonHoc?.TenMon,
                        GiangVien = lopHocHienTai?.GiangVien?.NguoiDung?.HoTen,
                        TongTinChi = sv.TongTinChi,
                        Gpa = sv.Gpa,
                        TrangThai = sv.NguoiDung != null ? GetTrangThaiString(sv.NguoiDung.TrangThai) : null,
                        DanhGia = GetDanhGia(sv.Gpa)
                    };
                }).ToList();

                var response = new TruongKhoaPagedResponse<HocVienListDTO>
                {
                    Data = new TruongKhoaPagedData<HocVienListDTO>
                    {
                        Items = hocViens,
                        Total = total,
                        Page = page,
                        Limit = limit,
                        TotalPages = totalPages
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách học viên");
                return StatusCode(500, new TruongKhoaPagedResponse<HocVienListDTO>
                {
                    Success = false,
                    Data = new TruongKhoaPagedData<HocVienListDTO> { Items = new List<HocVienListDTO>() }
                });
            }
        }

        // 3. CHI TIẾT HỌC VIÊN
        [HttpGet("{id}")]
        public async Task<ActionResult<TruongKhoaApiResponse<HocVienDetailResponseDTO>>> GetChiTietHocVien(int id)
        {
            try
            {
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                var sinhVien = await _context.HoSoSinhViens
                    .Include(sv => sv.NguoiDung)
                    .Include(sv => sv.Nganh)
                    .ThenInclude(n => n.Khoa)
                    .Include(sv => sv.KhoaTuyenSinh)
                    .FirstOrDefaultAsync(sv => sv.SinhVienId == id && 
                        sv.Nganh != null && sv.Nganh.KhoaId == khoaIdTruongKhoa.Value);

                if (sinhVien == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy học viên" });
                }

                // Thông tin cá nhân
                var thongTinCaNhan = new ThongTinCaNhanDTO
                {
                    Id = sinhVien.SinhVienId,
                    Mssv = sinhVien.Mssv,
                    HoTen = sinhVien.NguoiDung?.HoTen,
                    GioiTinh = sinhVien.NguoiDung?.GioiTinh,
                    NgaySinh = sinhVien.NguoiDung?.NgaySinh.HasValue == true 
                        ? sinhVien.NguoiDung.NgaySinh.Value.ToString("dd/MM/yyyy") : null,
                    DienThoai = sinhVien.NguoiDung?.SoDienThoai,
                    Email = sinhVien.NguoiDung?.Email,
                    Avatar = sinhVien.NguoiDung?.Avatar,
                    ChuyenNganh = sinhVien.Nganh?.TenNganh,
                    Khoa = sinhVien.KhoaTuyenSinh?.TenKhoaTuyenSinh,
                    TongTinChi = sinhVien.TongTinChi,
                    NgayTaoHoSo = sinhVien.CreatedAt.HasValue 
                        ? sinhVien.CreatedAt.Value.ToString("dd/MM/yyyy") : null,
                    TrangThai = sinhVien.NguoiDung != null ? GetTrangThaiString(sinhVien.NguoiDung.TrangThai) : null,
                    GpaTichLuy = sinhVien.Gpa
                };

                // Bảng điểm môn học
                var bangDiemMonHoc = await GetBangDiemMonHoc(id);

                var response = new HocVienDetailResponseDTO
                {
                    ThongTinCaNhan = thongTinCaNhan,
                    BangDiemMonHoc = bangDiemMonHoc
                };

                return Ok(new TruongKhoaApiResponse<HocVienDetailResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy chi tiết học viên ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 4. BẢNG ĐIỂM SINH VIÊN
        [HttpGet("{id}/bang-diem")]
        public async Task<ActionResult<TruongKhoaApiResponse<List<BangDiemMonHocDTO>>>> GetBangDiemSinhVien(int id)
        {
            try
            {
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                var sinhVien = await _context.HoSoSinhViens
                    .Include(sv => sv.Nganh)
                    .FirstOrDefaultAsync(sv => sv.SinhVienId == id && 
                        sv.Nganh != null && sv.Nganh.KhoaId == khoaIdTruongKhoa.Value);

                if (sinhVien == null)
                {
                    return NotFound(new TruongKhoaApiResponse<object> { Success = false, Message = "Không tìm thấy học viên" });
                }

                var bangDiem = await GetBangDiemMonHoc(id);

                return Ok(new TruongKhoaApiResponse<List<BangDiemMonHocDTO>>
                {
                    Success = true,
                    Data = bangDiem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy bảng điểm học viên ID: {id}");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // 5. LỊCH SỬ ĐIỂM
        [HttpGet("{id}/lich-su-diem")]
        public async Task<ActionResult<TruongKhoaPagedResponse<LichSuDiemDTO>>> GetLichSuDiem(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] int? monHocId = null,
            [FromQuery] DateTime? tuNgay = null,
            [FromQuery] DateTime? denNgay = null)
        {
            try
            {
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaPagedResponse<LichSuDiemDTO>
                    {
                        Success = false,
                        Data = new TruongKhoaPagedData<LichSuDiemDTO> { Items = new List<LichSuDiemDTO>() }
                    });
                }

                var sinhVien = await _context.HoSoSinhViens
                    .Include(sv => sv.Nganh)
                    .FirstOrDefaultAsync(sv => sv.SinhVienId == id && 
                        sv.Nganh != null && sv.Nganh.KhoaId == khoaIdTruongKhoa.Value);

                if (sinhVien == null)
                {
                    return NotFound(new TruongKhoaPagedResponse<LichSuDiemDTO>
                    {
                        Success = false,
                        Data = new TruongKhoaPagedData<LichSuDiemDTO> { Items = new List<LichSuDiemDTO>() }
                    });
                }

                var query = _context.LichSuSuaDiems
                    .Include(ls => ls.Diem)
                    .ThenInclude(d => d.LopHoc)
                    .ThenInclude(l => l.MonHoc)
                    .Include(ls => ls.NguoiDung)
                    .Where(ls => ls.Diem.SinhVienId == id);

                if (monHocId.HasValue)
                {
                    query = query.Where(ls => ls.Diem.LopHoc.MonHocId == monHocId.Value);
                }

                if (tuNgay.HasValue)
                {
                    query = query.Where(ls => ls.CreatedAt >= tuNgay.Value);
                }

                if (denNgay.HasValue)
                {
                    query = query.Where(ls => ls.CreatedAt <= denNgay.Value);
                }

                var total = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(total / (double)limit);

                var lichSu = await query
                    .OrderByDescending(ls => ls.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(ls => new LichSuDiemDTO
                    {
                        ThoiGian = ls.CreatedAt.HasValue ? ls.CreatedAt.Value.ToString("dd/MM/yyyy hh:mm tt") : "",
                        NguoiThucHien = ls.NguoiDung != null ? ls.NguoiDung.HoTen : "Hệ thống",
                        MonHoc = ls.Diem.LopHoc.MonHoc.TenMon,
                        ThanhPhanDiem = "Điểm trung bình môn", // Có thể lấy từ DiemThanhPhan nếu cần
                        DiemCu = ls.DiemLucDau,
                        DiemMoi = ls.DiemMoi,
                        LyDo = ls.LyDo
                    })
                    .ToListAsync();

                var response = new TruongKhoaPagedResponse<LichSuDiemDTO>
                {
                    Data = new TruongKhoaPagedData<LichSuDiemDTO>
                    {
                        Items = lichSu,
                        Total = total,
                        Page = page,
                        Limit = limit,
                        TotalPages = totalPages
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy lịch sử điểm học viên ID: {id}");
                return StatusCode(500, new TruongKhoaPagedResponse<LichSuDiemDTO>
                {
                    Success = false,
                    Data = new TruongKhoaPagedData<LichSuDiemDTO> { Items = new List<LichSuDiemDTO>() }
                });
            }
        }

        // 6. TÌM KIẾM HỌC VIÊN
        [HttpGet("tim-kiem")]
        public async Task<ActionResult<TruongKhoaPagedResponse<HocVienListDTO>>> TimKiemHocVien(
            [FromQuery] TimKiemHocVienDTO timKiemDTO)
        {
            return await GetDanhSachHocVien(
                timKiemDTO.Page,
                timKiemDTO.Limit,
                timKiemDTO.Search,
                timKiemDTO.KhoaId,
                timKiemDTO.NganhId,
                timKiemDTO.KhoaTuyenSinhId,
                timKiemDTO.TrangThai);
        }

        // 7. LỌC HỌC VIÊN
        [HttpGet("filter")]
        public async Task<ActionResult<TruongKhoaPagedResponse<HocVienListDTO>>> FilterHocVien(
            [FromQuery] TimKiemHocVienDTO filterDTO)
        {
            return await GetDanhSachHocVien(
                filterDTO.Page,
                filterDTO.Limit,
                filterDTO.Search,
                filterDTO.KhoaId,
                filterDTO.NganhId,
                filterDTO.KhoaTuyenSinhId,
                filterDTO.TrangThai);
        }

        // 8. XUẤT BÁO CÁO
        [HttpGet("xuat-bao-cao")]
        public async Task<IActionResult> XuatBaoCao(
            [FromQuery] string format = "pdf",
            [FromQuery] string loaiBaoCao = "danh-sach",
            [FromQuery] string? ids = null,
            [FromQuery] int? khoaId = null,
            [FromQuery] int? nganhId = null,
            [FromQuery] int? khoaTuyenSinhId = null)
        {
            try
            {
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new { success = false, message = "Bạn không phải là Trưởng khoa" });
                }

                // TODO: Implement logic xuất báo cáo PDF/Excel
                // Hiện tại trả về thông báo chưa implement
                return BadRequest(new { success = false, message = "Chức năng xuất báo cáo đang được phát triển" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất báo cáo");
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi", error = ex.Message });
            }
        }

        // 9. TẠO BÁO CÁO
        [HttpPost("xuat-bao-cao/tao")]
        public async Task<ActionResult<TruongKhoaApiResponse<object>>> TaoBaoCao([FromBody] XuatBaoCaoRequestDTO request)
        {
            try
            {
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // TODO: Implement logic tạo báo cáo
                return Ok(new TruongKhoaApiResponse<object>
                {
                    Success = true,
                    Message = "Báo cáo đang được tạo",
                    Data = new { message = "Chức năng đang được phát triển" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo báo cáo");
                return StatusCode(500, new TruongKhoaApiResponse<object> { Success = false, Message = "Đã xảy ra lỗi", Data = ex.Message });
            }
        }

        // ============ PHƯƠNG THỨC HỖ TRỢ ============

        private async Task<List<BangDiemMonHocDTO>> GetBangDiemMonHoc(int sinhVienId)
        {
            var diems = await _context.Diems
                .Include(d => d.LopHoc)
                .ThenInclude(l => l.MonHoc)
                .Where(d => d.SinhVienId == sinhVienId)
                .ToListAsync();

            var bangDiem = new List<BangDiemMonHocDTO>();

            foreach (var diem in diems)
            {
                // Lấy các thành phần điểm
                var diemThanhPhans = await _context.DiemThanhPhans
                    .Include(dtp => dtp.ThanhPhanDiem)
                    .Where(dtp => dtp.SinhVienId == sinhVienId && dtp.LopHocId == diem.LopHocId)
                    .ToListAsync();

                var diemBaiTap = diemThanhPhans
                    .FirstOrDefault(dtp => dtp.ThanhPhanDiem.Ten.Contains("Bài tập"))?.Diem;

                var diemGiuaKy = diemThanhPhans
                    .FirstOrDefault(dtp => dtp.ThanhPhanDiem.Ten.Contains("Giữa kỳ"))?.Diem;

                var diemCuoiKy = diemThanhPhans
                    .FirstOrDefault(dtp => dtp.ThanhPhanDiem.Ten.Contains("Cuối kỳ"))?.Diem;

                // Lấy điểm chuyên cần
                var diemChuyenCan = await _context.DiemChuyenCans
                    .FirstOrDefaultAsync(dcc => dcc.SinhVienId == sinhVienId && dcc.LopHocId == diem.LopHocId);

                bangDiem.Add(new BangDiemMonHocDTO
                {
                    MonHoc = diem.LopHoc?.MonHoc?.TenMon,
                    DiemBaiTap = diemBaiTap,
                    DiemGiuaKy = diemGiuaKy,
                    DiemCuoiKy = diemCuoiKy,
                    DiemChuyenCan = diemChuyenCan?.Diem,
                    DiemTrungBinhMon = diem.DiemTrungBinhMon,
                    DiemChu = diem.DiemChu,
                    GpaMon = diem.Gpamon
                });
            }

            return bangDiem;
        }

        private string GetTrangThaiString(byte? trangThai)
        {
            return trangThai switch
            {
                1 => "Đang học",
                2 => "Đã tốt nghiệp",
                3 => "Đã nghỉ",
                4 => "Cảnh báo",
                0 => "Bị khóa",
                _ => "Không xác định"
            };
        }

        private string GetDanhGia(decimal? gpa)
        {
            if (!gpa.HasValue) return "Chưa có đánh giá";
            
            return gpa.Value switch
            {
                >= 3.5m => "Xuất sắc",
                >= 3.0m => "Tốt",
                >= 2.5m => "Khá",
                >= 2.0m => "Trung bình",
                _ => "Cần cải thiện"
            };
        }
    }
}

