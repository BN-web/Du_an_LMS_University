using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using LMS_GV.DTOs.TruongKhoa;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace LMS_GV.Controllers.TruongKhoa
{
    [Route("api/truong-khoa/hoc-vien")]
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
            
            // Kiểm tra VaiTroId trước
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.NguoiDungId == truongKhoaId);
            
            if (nguoiDung?.VaiTroId != 3)
            {
                return null;
            }
            
            var giangVien = await _context.GiangViens
                .FirstOrDefaultAsync(gv => gv.NguoiDungId == truongKhoaId);

            // Nếu chưa có GiangVien, tạo mới
            if (giangVien == null)
            {
                giangVien = new GiangVien
                {
                    NguoiDungId = truongKhoaId,
                    ChucVu = "Trưởng khoa",
                    KhoaId = null,
                    CreatedAt = DateTime.Now
                };
                _context.GiangViens.Add(giangVien);
                await _context.SaveChangesAsync();
            }
            else if (giangVien.ChucVu != "Trưởng khoa")
            {
                // Đảm bảo ChucVu đúng
                giangVien.ChucVu = "Trưởng khoa";
                giangVien.UpdatedAt = DateTime.Now;
                _context.GiangViens.Update(giangVien);
                await _context.SaveChangesAsync();
            }

            return giangVien.KhoaId;
        }

        // 1. THỐNG KÊ TỔNG QUAN
        [HttpGet("thong-ke")]
        public async Task<ActionResult<TruongKhoaApiResponse<ThongKeHocVienResponseDTO>>> GetThongKe()
        {
            try
            {
                var truongKhoaId = GetTruongKhoaId();
                
                // Kiểm tra VaiTroId trước - chỉ cần VaiTroId == 3 là đủ
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(nd => nd.NguoiDungId == truongKhoaId);
                
                if (nguoiDung?.VaiTroId != 3)
                {
                    return Unauthorized(new TruongKhoaApiResponse<object> { Success = false, Message = "Bạn không phải là Trưởng khoa" });
                }

                // Đảm bảo có GiangVien record
                var giangVien = await _context.GiangViens
                    .FirstOrDefaultAsync(gv => gv.NguoiDungId == truongKhoaId);

                if (giangVien == null)
                {
                    giangVien = new GiangVien
                    {
                        NguoiDungId = truongKhoaId,
                        ChucVu = "Trưởng khoa",
                        KhoaId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.GiangViens.Add(giangVien);
                    await _context.SaveChangesAsync();
                }

                var khoaIdTruongKhoa = giangVien.KhoaId;

                // Lấy tất cả học viên thuộc khoa (nếu có khoaId, nếu không thì lấy tất cả học viên)
                var queryHocViens = _context.HoSoSinhViens
                    .Include(sv => sv.Nganh)
                    .AsQueryable();
                
                if (khoaIdTruongKhoa.HasValue)
                {
                    queryHocViens = queryHocViens.Where(sv => sv.Nganh != null && sv.Nganh.KhoaId == khoaIdTruongKhoa.Value);
                }
                
                var hocViens = await queryHocViens.ToListAsync();

                var tongSoHocVien = hocViens.Count;
                var hocVienHoatDong = hocViens.Count(sv => sv.NguoiDung != null && sv.NguoiDung.TrangThai == 1);
                var hocVienDaNghi = hocViens.Count(sv => sv.NguoiDung != null && sv.NguoiDung.TrangThai == 3);

                // Đếm số lớp học thuộc khoa (nếu có khoaId)
                var queryLopHoc = _context.LopHocs.Where(l => l.TrangThai == 1).AsQueryable();
                if (khoaIdTruongKhoa.HasValue)
                {
                    queryLopHoc = queryLopHoc.Where(l => l.KhoaId == khoaIdTruongKhoa.Value);
                }
                var tongSoLop = await queryLopHoc.CountAsync();

                // Đếm số giảng viên thuộc khoa (nếu có khoaId)
                var queryGiangVien = _context.GiangViens.AsQueryable();
                if (khoaIdTruongKhoa.HasValue)
                {
                    queryGiangVien = queryGiangVien.Where(gv => gv.KhoaId == khoaIdTruongKhoa.Value);
                }
                var tongGiangVien = await queryGiangVien.CountAsync();

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

                // Bảng điểm hoàn thành bài tập (lấy từ các lớp thuộc khoa, nếu có khoaId)
                var queryLopHocForChart = _context.LopHocs
                    .Include(l => l.MonHoc)
                    .Where(l => l.TrangThai == 1)
                    .AsQueryable();
                
                if (khoaIdTruongKhoa.HasValue)
                {
                    queryLopHocForChart = queryLopHocForChart.Where(l => l.KhoaId == khoaIdTruongKhoa.Value);
                }
                
                var lopHocs = await queryLopHocForChart.ToListAsync();

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
                // 1. Validate page / limit
                if (page < 1) page = 1;
                if (limit < 1) limit = 10;
                if (limit > 100) limit = 100; // Giới hạn tối đa

                // 2. Lấy KhoaId trưởng khoa (safe)
                var khoaIdTruongKhoa = await GetKhoaIdOfTruongKhoa();
                
                // Nếu không có KhoaId hoặc không phải trưởng khoa → trả danh sách rỗng (200)
                if (!khoaIdTruongKhoa.HasValue)
                {
                    return Ok(new TruongKhoaPagedResponse<HocVienListDTO>
                    {
                        Success = true,
                        Data = new TruongKhoaPagedData<HocVienListDTO>
                        {
                            Items = new List<HocVienListDTO>(),
                            Total = 0,
                            Page = page,
                            Limit = limit,
                            TotalPages = 0
                        }
                    });
                }

                // 3. Query base (filter + count)
                var baseQuery = _context.HoSoSinhViens
                    .Include(sv => sv.Nganh)
                    .Where(sv => sv.Nganh != null && sv.Nganh.KhoaId == khoaIdTruongKhoa.Value)
                    .AsQueryable();

                // Áp dụng các filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    baseQuery = baseQuery.Where(sv =>
                        (sv.Mssv != null && sv.Mssv.Contains(search)) ||
                        (sv.NguoiDung != null &&
                         sv.NguoiDung.HoTen != null &&
                         sv.NguoiDung.HoTen.Contains(search))
                    );
                }

                if (nganhId.HasValue)
                {
                    baseQuery = baseQuery.Where(sv => sv.NganhId == nganhId.Value);
                }

                if (khoaTuyenSinhId.HasValue)
                {
                    baseQuery = baseQuery.Where(sv => sv.KhoaTuyenSinhId == khoaTuyenSinhId.Value);
                }

                if (trangThai.HasValue)
                {
                    baseQuery = baseQuery.Where(sv =>
                        sv.NguoiDung != null &&
                        sv.NguoiDung.TrangThai == trangThai.Value);
                }

                // Đếm tổng số bản ghi
                var total = await baseQuery.CountAsync();
                
                if (total == 0)
                {
                    return Ok(new TruongKhoaPagedResponse<HocVienListDTO>
                    {
                        Success = true,
                        Data = new TruongKhoaPagedData<HocVienListDTO>
                        {
                            Items = new List<HocVienListDTO>(),
                            Total = 0,
                            Page = page,
                            Limit = limit,
                            TotalPages = 0
                        }
                    });
                }

                // Ép page hợp lệ (chống FE ngu)
                var totalPages = (int)Math.Ceiling(total / (double)limit);
                if (page > totalPages && totalPages > 0)
                {
                    page = totalPages;
                }

                // 4. Query ID (pagination)
                var ids = await baseQuery
                    .OrderByDescending(sv => sv.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(sv => sv.SinhVienId)
                    .ToListAsync();

                // 5. Query chi tiết theo ID - tách query để tránh lỗi SQL
                var data = await _context.HoSoSinhViens
                    .AsNoTracking()
                    .Include(sv => sv.NguoiDung)
                    .Include(sv => sv.Nganh)
                        .ThenInclude(n => n.Khoa)
                    .Include(sv => sv.KhoaTuyenSinh)
                    .Where(sv => ids.Contains(sv.SinhVienId))
                    .ToListAsync();

                // Load SinhVienLops riêng để tránh lỗi SQL với nhiều nested includes
                var sinhVienIds = data.Select(sv => sv.SinhVienId).ToList();
                var sinhVienLops = await _context.SinhVienLops
                    .AsNoTracking()
                    .Include(sl => sl.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(sl => sl.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                        .ThenInclude(gv => gv.NguoiDung)
                    .Where(sl => sinhVienIds.Contains(sl.SinhVienId))
                    .ToListAsync();

                // Tạo dictionary để lookup nhanh
                var sinhVienLopsDict = sinhVienLops
                    .GroupBy(sl => sl.SinhVienId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 6. Map DTO → trả response
                var result = data.Select(sv =>
                {
                    // Lookup SinhVienLops từ dictionary
                    var svLops = sinhVienLopsDict.TryGetValue(sv.SinhVienId, out var lopList) 
                        ? lopList 
                        : new List<SinhVienLop>();
                    
                    var lopDangHoc = svLops
                        .FirstOrDefault(x => x.TinhTrang == "dang hoc")?.LopHoc;

                    return new HocVienListDTO
                    {
                        Id = sv.SinhVienId,
                        Mssv = sv.Mssv,
                        HoTen = sv.NguoiDung?.HoTen,
                        TenKhoa = sv.Nganh?.Khoa?.TenKhoa,
                        Nganh = sv.Nganh?.TenNganh,
                        Khoa = sv.KhoaTuyenSinh?.TenKhoaTuyenSinh,
                        MonLop = lopDangHoc?.MonHoc?.TenMon,
                        GiangVien = lopDangHoc?.GiangVien?.NguoiDung?.HoTen,
                        TongTinChi = sv.TongTinChi,
                        Gpa = sv.Gpa,
                        TrangThai = sv.NguoiDung != null ? GetTrangThaiString(sv.NguoiDung.TrangThai) : null,
                        DanhGia = GetDanhGia(sv.Gpa)
                    };
                }).ToList();

                return Ok(new TruongKhoaPagedResponse<HocVienListDTO>
                {
                    Success = true,
                    Data = new TruongKhoaPagedData<HocVienListDTO>
                    {
                        Items = result,
                        Total = total,
                        Page = page,
                        Limit = limit,
                        TotalPages = (int)Math.Ceiling(total / (double)limit)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetDanhSachHocVien FAILED | page={Page}, limit={Limit}, search={Search}",
                    page, limit, search);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    stack = ex.StackTrace
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
                    .ThenInclude(n => n != null ? n.Khoa : null)
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
                    .ThenInclude(d => d != null ? d.LopHoc : null)
                    .ThenInclude(l => l != null ? l.MonHoc : null)
                    .Include(ls => ls.NguoiDung)
                    .Where(ls => ls.Diem != null && ls.Diem.SinhVienId == id);

                if (monHocId.HasValue)
                {
                    query = query.Where(ls => ls.Diem != null && ls.Diem.LopHoc != null && ls.Diem.LopHoc.MonHocId == monHocId.Value);
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
                        MonHoc = ls.Diem != null && ls.Diem.LopHoc != null && ls.Diem.LopHoc.MonHoc != null ? ls.Diem.LopHoc.MonHoc.TenMon : "",
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
                .ThenInclude(l => l != null ? l.MonHoc : null)
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
                    .FirstOrDefault(dtp => dtp.ThanhPhanDiem != null && dtp.ThanhPhanDiem.Ten.Contains("Bài tập"))?.Diem;

                var diemGiuaKy = diemThanhPhans
                    .FirstOrDefault(dtp => dtp.ThanhPhanDiem != null && dtp.ThanhPhanDiem.Ten.Contains("Giữa kỳ"))?.Diem;

                var diemCuoiKy = diemThanhPhans
                    .FirstOrDefault(dtp => dtp.ThanhPhanDiem != null && dtp.ThanhPhanDiem.Ten.Contains("Cuối kỳ"))?.Diem;

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
