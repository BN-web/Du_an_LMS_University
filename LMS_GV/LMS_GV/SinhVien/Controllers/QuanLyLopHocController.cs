using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LMS_GV.SinhVien.DTOs;
using LMS_GV.Models.Data;
using System.Text.Json;
using LMS_GV.Models;

namespace LMS_GV.SinhVien.Controllers
{
    [Route("api/sinhvien/lop-hoc")]
    [ApiController]
    public class QuanLyLopHocController : ControllerBase
    {
        private readonly AppDbContext _context;
        public QuanLyLopHocController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: Lấy SinhVienId từ token
        private async Task<int?> GetSinhVienId()
        {
            var nguoiDungIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nguoiDungIdClaim) || !int.TryParse(nguoiDungIdClaim, out int nguoiDungId))
            {
                return null;
            }
            var sinhVien = await _context.HoSoSinhViens
                .FirstOrDefaultAsync(h => h.NguoiDungId == nguoiDungId);
            return sinhVien?.SinhVienId;
        }

        // Helper: Kiểm tra sinh viên có thuộc lớp không
        private async Task<bool> KiemTraSinhVienThuocLop(int sinhVienId, int lopHocId)
        {
            return await _context.SinhVienLops
                .AnyAsync(svl => svl.SinhVienId == sinhVienId && svl.LopHocId == lopHocId);
        }

        // Helper: Chuyển trạng thái số sang chuỗi
        private string GetTrangThaiMonHoc(byte? trangThai)
        {
            return trangThai switch
            {
                1 => "Đậu",
                0 => "Rớt",
                _ => "Chưa có điểm"
            };
        }

        // 1. GET: /api/sinhvien/lop-hoc
        [HttpGet]
        public async Task<IActionResult> GetDanhSachLopHoc()
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Lấy danh sách lớp học của sinh viên (include đầy đủ chain để tránh lazy loading nếu cần)
                var lopHocList = await _context.SinhVienLops
                    .Include(svl => svl.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                            .ThenInclude(g => g.NguoiDung)  // Giữ include này để an toàn, hoặc load riêng nếu tối ưu
                    .Include(svl => svl.LopHoc)
                        .ThenInclude(l => l.Nganh)
                    .Include(svl => svl.LopHoc)
                        .ThenInclude(l => l.HocKy)
                    .Where(svl => svl.SinhVienId == sinhVienId && svl.LopHoc != null)
                    .Select(svl => svl.LopHoc)
                    .ToListAsync();
                var danhSachLopHoc = lopHocList.Select(l => new LopHocItemDTO
                {
                    LopHocId = l.LopHocId,
                    MaLop = l.MaLop,
                    TenLop = l.TenLop,
                    GiangVien = l.GiangVien?.NguoiDung?.HoTen ?? "Chưa có giảng viên",
                    EmailGiangVien = l.GiangVien?.NguoiDung?.Email,
                    Nganh = l.Nganh?.TenNganh,
                    SoTinChi = l.SoTinChi,
                    HocKy = $"{l.HocKy?.KiHoc} {l.HocKy?.NamHoc}"
                }).ToList();
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Data = danhSachLopHoc
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 2. GET: /api/sinhvien/lop-hoc/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietLopHoc(int id)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy thông tin chi tiết lớp học (include đầy đủ để an toàn)
                var lopHoc = await _context.LopHocs
                    .Include(l => l.GiangVien)
                        .ThenInclude(g => g.NguoiDung)  // Giữ để tránh lỗi
                    .Include(l => l.Nganh)
                    .Include(l => l.MonHoc)
                    .Include(l => l.HocKy)
                    .Include(l => l.Khoa)
                    .FirstOrDefaultAsync(l => l.LopHocId == id);
                if (lopHoc == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy lớp học"
                    });
                }
                // Lấy số lượng bài tập và tài liệu
                var soBaiTap = await _context.BaiKiemTras
                    .CountAsync(b => b.LopHocId == id);
                var soTaiLieu = await _context.BaiHocLops
                    .CountAsync(b => b.LopHocId == id);
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Data = new
                    {
                        LopHocId = lopHoc.LopHocId,
                        MaLop = lopHoc.MaLop,
                        TenLop = lopHoc.TenLop,
                        GiangVien = lopHoc.GiangVien?.NguoiDung?.HoTen ?? "Chưa có giảng viên",
                        EmailGiangVien = lopHoc.GiangVien?.NguoiDung?.Email,
                        Nganh = lopHoc.Nganh?.TenNganh,
                        Khoa = lopHoc.Khoa?.TenKhoa,
                        MonHoc = lopHoc.MonHoc?.TenMon,
                        MaMon = lopHoc.MonHoc?.MaMon,
                        SoTinChi = lopHoc.SoTinChi,
                        HocKy = $"{lopHoc.HocKy?.KiHoc} {lopHoc.HocKy?.NamHoc}",
                        NgayBatDau = lopHoc.NgayBatDau?.ToString("dd/MM/yyyy"),
                        NgayKetThuc = lopHoc.NgayKetThuc?.ToString("dd/MM/yyyy"),
                        SiSo = lopHoc.SiSo,
                        SiSoToiDa = lopHoc.SiSoToiDa,
                        SoBaiTap = soBaiTap,
                        SoTaiLieu = soTaiLieu
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 3. GET: /api/sinhvien/lop-hoc/{id}/bai-tap
        [HttpGet("{id}/bai-tap")]
        public async Task<IActionResult> GetDanhSachBaiTap(int id)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy danh sách bài tập/kiểm tra của lớp
                var baiTapList = await _context.BaiKiemTras
                    .Include(b => b.MonHoc)
                    .Where(b => b.LopHocId == id)
                    .OrderByDescending(b => b.NgayKetThuc)
                    .ToListAsync();
                // Lấy danh sách bài nộp của sinh viên
                var baiNopList = await _context.BaiNops
                    .Where(bn => bn.SinhVienId == sinhVienId && bn.BaiKiemTra != null && bn.BaiKiemTra.LopHocId == id)
                    .Include(bn => bn.BaiKiemTra)
                    .ToListAsync();
                var danhSachBaiTap = new List<BaiTapItemDTO>();
                foreach (var baiTap in baiTapList)
                {
                    var baiNop = baiNopList.FirstOrDefault(bn => bn.BaiKiemTraId == baiTap.BaiKiemTraId);
                    bool daNop = baiNop != null;
                    string? giangVienName = "Chưa có giảng viên";
                    if (baiTap.GiangVienId.HasValue)
                    {
                        var giangVien = await _context.GiangViens
                            .Include(g => g.NguoiDung)
                            .FirstOrDefaultAsync(g => g.GiangVienId == baiTap.GiangVienId.Value);
                        giangVienName = giangVien?.NguoiDung?.HoTen;
                    }
                    var baiTapItem = new BaiTapItemDTO
                    {
                        BaiTapId = baiTap.BaiKiemTraId,
                        TieuDe = baiTap.TieuDe,
                        MonHoc = baiTap.MonHoc?.TenMon,
                        GiangVien = giangVienName,
                        ThoiGianNop = baiTap.NgayKetThuc?.ToString("dd/MM/yyyy HH:mm"),
                        TrangThai = daNop ? "Đã nộp" : "Chưa làm",
                        LoaiBai = baiTap.Loai,
                        DiemToiDa = baiTap.DiemToiDa,
                        ThoiGianLamBaiToiDa = baiTap.ThoiGianLamBai,
                        ChoPhepLamLai = baiTap.ChoPhepLamLai.GetValueOrDefault()
                    };
                    if (daNop)
                    {
                        baiTapItem.DiemDatDuoc = baiNop.TongDiem;
                        baiTapItem.ThoiGianNopBai = baiNop.NgayNop?.ToString("dd/MM/yyyy HH:mm");
                        baiTapItem.ThoiGianLamBai = baiNop.ThoiGianHoanThanh;
                    }
                    danhSachBaiTap.Add(baiTapItem);
                }
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Data = new { BaiTap = danhSachBaiTap }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 4. GET: /api/sinhvien/lop-hoc/{id}/bai-tap/{baiTapId}
        [HttpGet("{id}/bai-tap/{baiTapId}")]
        public async Task<IActionResult> GetChiTietBaiTap(int id, int baiTapId)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy thông tin bài kiểm tra
                var baiKiemTra = await _context.BaiKiemTras
                    .Include(b => b.MonHoc)
                    .Include(b => b.CauHois)
                        .ThenInclude(c => c.TuyChonCauHois)
                    .FirstOrDefaultAsync(b => b.BaiKiemTraId == baiTapId && b.LopHocId == id);
                if (baiKiemTra == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài tập"
                    });
                }
                // Nếu bài kiểm tra có random câu hỏi, chúng ta sẽ xử lý random ở đây
                var cauHoiList = baiKiemTra.CauHois.ToList();
                if (baiKiemTra.IsRandom.GetValueOrDefault() && baiKiemTra.SoCau.HasValue && baiKiemTra.SoCau.Value < cauHoiList.Count)
                {
                    var random = new Random();
                    cauHoiList = cauHoiList.OrderBy(x => random.Next()).Take(baiKiemTra.SoCau.Value).ToList();
                }
                var cauHoiResponse = cauHoiList.Select(c => new CauHoiDTO
                {
                    CauHoiId = c.CauHoiId,
                    NoiDung = c.NoiDung,
                    Loai = c.Loai,
                    Diem = c.Diem,
                    TuyChon = c.TuyChonCauHois.Select(t => new TuyChonCauHoiDTO
                    {
                        MaLuaChon = t.MaLuaChon,
                        NoiDung = t.NoiDung,
                        LaDapAn = t.LaDapAn
                    }).ToList()
                }).ToList();
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Data = new
                    {
                        BaiTapId = baiKiemTra.BaiKiemTraId,
                        TieuDe = baiKiemTra.TieuDe,
                        MonHoc = baiKiemTra.MonHoc?.TenMon,
                        ThoiGianLamBaiToiDa = baiKiemTra.ThoiGianLamBai,
                        SoCau = cauHoiResponse.Count,
                        CauHoi = cauHoiResponse
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 5. POST: /api/sinhvien/lop-hoc/{id}/bai-tap/{baiTapId}/bat-dau
        [HttpPost("{id}/bai-tap/{baiTapId}/bat-dau")]
        public async Task<IActionResult> BatDauBaiTap(int id, int baiTapId)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Kiểm tra bài kiểm tra có tồn tại không
                var baiKiemTra = await _context.BaiKiemTras.FindAsync(baiTapId);
                if (baiKiemTra == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài tập"
                    });
                }
                // Kiểm tra thời gian làm bài
                if (baiKiemTra.NgayKetThuc.HasValue && baiKiemTra.NgayKetThuc.Value < DateTime.Now)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Đã quá thời hạn nộp bài"
                    });
                }
                // Kiểm tra số lần làm bài
                var soLanDaNop = await _context.BaiNops
                    .CountAsync(bn => bn.SinhVienId == sinhVienId && bn.BaiKiemTraId == baiTapId);
                if (baiKiemTra.SoLanLamToiDa.HasValue && soLanDaNop >= baiKiemTra.SoLanLamToiDa.Value)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Đã đạt số lần làm bài tối đa"
                    });
                }
                // Tạo bản ghi bài nộp mới
                var baiNop = new BaiNop
                {
                    BaiKiemTraId = baiTapId,
                    SinhVienId = sinhVienId.Value,
                    NgayNop = DateTime.Now,
                    TongDiem = null,
                    TrangThai = 0, // Chưa chấm
                    LanLam = soLanDaNop + 1
                };
                _context.BaiNops.Add(baiNop);
                await _context.SaveChangesAsync();
                // Tạo bản ghi tiến độ
                var tienDo = new TienDo
                {
                    SinhVienId = sinhVienId.Value,
                    BaiKiemTraId = baiTapId,
                    SoCauDaLam = 0,
                    TrangThai = 0
                };
                _context.TienDos.Add(tienDo);
                await _context.SaveChangesAsync();
                // Tính thời gian còn lại
                long thoiGianConLai = 0;
                if (baiKiemTra.ThoiGianLamBai.HasValue)
                {
                    thoiGianConLai = baiKiemTra.ThoiGianLamBai.Value * 60; // Chuyển sang giây
                }
                var response = new BatDauBaiTapResponseDTO
                {
                    BaiNopId = baiNop.BaiNopId,
                    ThoiGianBatDau = baiNop.NgayNop,
                    ThoiGianConLai = thoiGianConLai
                };
                return Ok(new ApiResponseDTO<BatDauBaiTapResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 6. PUT: /api/sinhvien/lop-hoc/{id}/bai-tap/{baiTapId}/tien-do
        [HttpPut("{id}/bai-tap/{baiTapId}/tien-do")]
        public async Task<IActionResult> LuuTienDo(int id, int baiTapId, [FromBody] TienDoRequestDTO request)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Cập nhật tiến độ
                var tienDo = await _context.TienDos
                    .FirstOrDefaultAsync(t => t.SinhVienId == sinhVienId && t.BaiKiemTraId == baiTapId);
                if (tienDo == null)
                {
                    tienDo = new TienDo
                    {
                        SinhVienId = sinhVienId.Value,
                        BaiKiemTraId = baiTapId,
                        SoCauDaLam = request.SoCauDaLam,
                        TrangThai = 0,
                        UpdatedAt = DateTime.Now
                    };
                    _context.TienDos.Add(tienDo);
                }
                else
                {
                    tienDo.SoCauDaLam = request.SoCauDaLam;
                    tienDo.UpdatedAt = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Message = "Đã lưu tiến độ"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 7. POST: /api/sinhvien/lop-hoc/{id}/bai-tap/{baiTapId}/nop-bai
        [HttpPost("{id}/bai-tap/{baiTapId}/nop-bai")]
        public async Task<IActionResult> NopBai(int id, int baiTapId, [FromBody] NopBaiRequestDTO request)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy thông tin bài nộp
                var baiNop = await _context.BaiNops
                    .FirstOrDefaultAsync(bn => bn.BaiNopId == request.BaiNopId && bn.SinhVienId == sinhVienId && bn.BaiKiemTraId == baiTapId);
                if (baiNop == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài nộp"
                    });
                }
                // Tính thời gian làm bài
                var thoiGianLamBai = 0;
                if (baiNop.NgayNop.HasValue)
                {
                    thoiGianLamBai = (int)(DateTime.Now - baiNop.NgayNop.Value).TotalMinutes;
                }
                // Cập nhật thông tin bài nộp
                baiNop.NgayNop = DateTime.Now;
                baiNop.ThoiGianHoanThanh = thoiGianLamBai;
                baiNop.TrangThai = 1; // Đã nộp
                // Lưu chi tiết câu trả lời
                if (request.CauTraLoi != null)
                {
                    // Xóa các câu trả lời cũ nếu có
                    var chiTietCu = _context.ChiTietCauTraLois.Where(ct => ct.BaiNopId == baiNop.BaiNopId);
                    _context.ChiTietCauTraLois.RemoveRange(chiTietCu);
                    foreach (var cauTraLoi in request.CauTraLoi)
                    {
                        var chiTiet = new ChiTietCauTraLoi
                        {
                            BaiNopId = baiNop.BaiNopId,
                            CauHoiId = cauTraLoi.CauHoiId,
                            LuaChonSinhVien = cauTraLoi.LuaChonSinhVien != null ? string.Join(",", cauTraLoi.LuaChonSinhVien) : null,
                            Diem = null // Chưa chấm
                        };
                        _context.ChiTietCauTraLois.Add(chiTiet);
                    }
                }
                // Tính điểm tự động nếu bài trắc nghiệm
                var baiKiemTra = await _context.BaiKiemTras
                    .Include(b => b.CauHois)
                        .ThenInclude(c => c.TuyChonCauHois)
                    .FirstOrDefaultAsync(b => b.BaiKiemTraId == baiTapId);
                decimal tongDiem = 0;
                if (baiKiemTra != null && request.CauTraLoi != null)
                {
                    foreach (var cauTraLoi in request.CauTraLoi)
                    {
                        var cauHoi = baiKiemTra.CauHois.FirstOrDefault(c => c.CauHoiId == cauTraLoi.CauHoiId);
                        if (cauHoi != null)
                        {
                            var dapAnDung = cauHoi.TuyChonCauHois.Where(t => t.LaDapAn).Select(t => t.MaLuaChon).ToList();
                            var dapAnSV = cauTraLoi.LuaChonSinhVien;
                            // So sánh đáp án
                            bool dung = false;
                            if (cauHoi.Loai == "single")
                            {
                                dung = dapAnSV != null && dapAnSV.Count == 1 && dapAnSV[0] == dapAnDung.FirstOrDefault();
                            }
                            else if (cauHoi.Loai == "multiple")
                            {
                                dung = dapAnSV != null && dapAnSV.OrderBy(x => x).SequenceEqual(dapAnDung.OrderBy(x => x));
                            }
                            if (dung)
                            {
                                tongDiem += cauHoi.Diem ?? 0;
                            }
                        }
                    }
                    baiNop.TongDiem = tongDiem;
                    // Lưu kết quả kiểm tra
                    var ketQua = new KetQuaKiemTra
                    {
                        BaiKiemTraId = baiTapId,
                        SinhVienId = sinhVienId.Value,
                        TongDiem = tongDiem,
                        ThoiGianLamBai = thoiGianLamBai,
                        IsAutoGraded = true
                    };
                    _context.KetQuaKiemTras.Add(ketQua);
                }
                // Cập nhật tiến độ thành hoàn thành
                var tienDo = await _context.TienDos
                    .FirstOrDefaultAsync(t => t.SinhVienId == sinhVienId && t.BaiKiemTraId == baiTapId);
                if (tienDo != null)
                {
                    tienDo.TrangThai = 1; // Hoàn thành
                    tienDo.UpdatedAt = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                var response = new NopBaiResponseDTO
                {
                    BaiNopId = baiNop.BaiNopId,
                    ThoiGianNop = baiNop.NgayNop,
                    SoCauDaTraLoi = request.CauTraLoi?.Count ?? 0,
                    TongSoCau = baiKiemTra?.CauHois.Count ?? 0
                };
                return Ok(new ApiResponseDTO<NopBaiResponseDTO>
                {
                    Success = true,
                    Message = "Nộp bài thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 8. GET: /api/sinhvien/lop-hoc/{id}/bai-tap/{baiTapId}/ket-qua
        [HttpGet("{id}/bai-tap/{baiTapId}/ket-qua")]
        public async Task<IActionResult> GetKetQuaBaiTap(int id, int baiTapId)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy bài kiểm tra
                var baiKiemTra = await _context.BaiKiemTras
                    .Include(b => b.MonHoc)
                    .FirstOrDefaultAsync(b => b.BaiKiemTraId == baiTapId);
                if (baiKiemTra == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài kiểm tra"
                    });
                }
                // Lấy bài nộp của sinh viên
                var baiNop = await _context.BaiNops
                    .Include(bn => bn.ChiTietCauTraLois)
                    .FirstOrDefaultAsync(bn => bn.SinhVienId == sinhVienId && bn.BaiKiemTraId == baiTapId);
                if (baiNop == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Bạn chưa làm bài này"
                    });
                }
                // Lấy chi tiết câu hỏi và đáp án
                var chiTietCauTraLoi = new List<ChiTietCauTraLoiDTO>();
                if (baiNop.ChiTietCauTraLois.Any())
                {
                    foreach (var ct in baiNop.ChiTietCauTraLois)
                    {
                        var cauHoi = await _context.CauHois
                            .Include(c => c.TuyChonCauHois)
                            .FirstOrDefaultAsync(c => c.CauHoiId == ct.CauHoiId);
                        if (cauHoi != null)
                        {
                            var dapAnDung = cauHoi.TuyChonCauHois
                                .Where(t => t.LaDapAn)
                                .Select(t => t.MaLuaChon)
                                .ToList();
                            var dapAnSV = ct.LuaChonSinhVien?.Split(',')?.ToList();
                            chiTietCauTraLoi.Add(new ChiTietCauTraLoiDTO
                            {
                                CauHoiId = ct.CauHoiId,
                                NoiDungCauHoi = cauHoi.NoiDung,
                                LuaChonSinhVien = dapAnSV,
                                DapAnDung = dapAnDung,
                                Diem = ct.Diem,
                                Dung = dapAnSV != null && dapAnDung.OrderBy(x => x).SequenceEqual(dapAnSV.OrderBy(x => x))
                            });
                        }
                    }
                }
                var response = new KetQuaBaiTapResponseDTO
                {
                    BaiNopId = baiNop.BaiNopId,
                    TieuDe = baiKiemTra.TieuDe,
                    DiemDatDuoc = baiNop.TongDiem,
                    DiemToiDa = baiKiemTra.DiemToiDa,
                    ThoiGianLamBai = baiNop.ThoiGianHoanThanh,
                    ThoiGianLamBaiToiDa = baiKiemTra.ThoiGianLamBai,
                    SoLanLam = baiNop.LanLam,
                    ChoPhepLamLai = baiKiemTra.ChoPhepLamLai.GetValueOrDefault(),
                    ChiTietCauTraLoi = chiTietCauTraLoi
                };
                return Ok(new ApiResponseDTO<KetQuaBaiTapResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 9. POST: /api/sinhvien/lop-hoc/{id}/bai-tap/{baiTapId}/bao-cao-gian-lan
        [HttpPost("{id}/bai-tap/{baiTapId}/bao-cao-gian-lan")]
        public async Task<IActionResult> BaoCaoGianLan(int id, int baiTapId, [FromBody] BaoCaoGianLanRequestDTO request)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy bài nộp
                var baiNop = await _context.BaiNops
                    .FirstOrDefaultAsync(bn => bn.BaiNopId == request.BaiNopId && bn.SinhVienId == sinhVienId && bn.BaiKiemTraId == baiTapId);
                if (baiNop == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài nộp"
                    });
                }
                // Cập nhật trạng thái gian lận
                baiNop.TrangThai = 0; // Đánh dấu là gian lận
                // Ghi log gian lận
                var nhatKy = new NhatKyHeThong
                {
                    NguoiDungId = sinhVienId,
                    HanhDong = $"Báo cáo gian lận bài tập: {baiTapId}",
                    ThamChieu = $"BaiNopId: {baiNop.BaiNopId}, Lý do: {request.LyDo}"
                };
                _context.NhatKyHeThongs.Add(nhatKy);
                await _context.SaveChangesAsync();
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Message = "Đã báo cáo gian lận thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 10. GET: /api/sinhvien/lop-hoc/{id}/bang-diem
        [HttpGet("{id}/bang-diem")]
        public async Task<IActionResult> GetBangDiem(int id)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy điểm của sinh viên trong lớp
                var diemList = await _context.Diems
                    .Include(d => d.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Where(d => d.SinhVienId == sinhVienId && d.LopHocId == id)
                    .ToListAsync();
                var bangDiem = diemList.Select(d => new BangDiemItemDTO
                {
                    MaMon = d.LopHoc?.MonHoc?.MaMon,
                    TenMon = d.LopHoc?.MonHoc?.TenMon,
                    DiemTrungBinh = d.DiemTrungBinhMon,
                    GpaMon = d.Gpamon,
                    SoTinChi = d.SoTinChi,
                    TrangThai = GetTrangThaiMonHoc(d.TrangThai),
                    DiemChu = d.DiemChu
                }).ToList();
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Data = new { BangDiem = bangDiem }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 11. GET: /api/sinhvien/lop-hoc/{id}/bang-diem/chi-tiet
        [HttpGet("{id}/bang-diem/chi-tiet")]
        public async Task<IActionResult> GetChiTietBangDiem(int id)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy điểm chi tiết
                var diem = await _context.Diems
                    .Include(d => d.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .FirstOrDefaultAsync(d => d.SinhVienId == sinhVienId && d.LopHocId == id);
                if (diem == null)
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy điểm"
                    });
                }
                // Lấy điểm thành phần
                var diemThanhPhanList = await _context.DiemThanhPhans
                    .Include(dtp => dtp.ThanhPhanDiem)
                    .Where(dtp => dtp.SinhVienId == sinhVienId && dtp.LopHocId == id)
                    .ToListAsync();
                // Lấy điểm chuyên cần
                var diemChuyenCan = await _context.DiemChuyenCans
                    .FirstOrDefaultAsync(dcc => dcc.SinhVienId == sinhVienId && dcc.LopHocId == id);
                // Lấy thông tin buổi vắng
                var soBuoiVang = await _context.DiemDanhChiTiets
                    .Include(dd => dd.DiemDanh)
                    .CountAsync(dd => dd.SinhVienId == sinhVienId && dd.TrangThai == "vang" && dd.DiemDanh.LopHocId == id);
                // Tạo response
                var response = new BangDiemChiTietDTO
                {
                    MaMon = diem.LopHoc?.MonHoc?.MaMon,
                    TenMon = diem.LopHoc?.MonHoc?.TenMon,
                    TrangThai = GetTrangThaiMonHoc(diem.TrangThai),
                    SoBuoiVang = soBuoiVang,
                    SoTinChi = diem.SoTinChi,
                    HeSo = diem.HeSoMon,
                    GpaMon = diem.Gpamon,
                    DiemTrungBinh = diem.DiemTrungBinhMon,
                    DiemChu = diem.DiemChu,
                    DiemChuyenCan = diemChuyenCan?.Diem,
                    DiemBaiTap = diemThanhPhanList.FirstOrDefault(dtp => dtp.ThanhPhanDiem?.Ten == "Bài tập")?.Diem,
                    DiemGiuaKy = diemThanhPhanList.FirstOrDefault(dtp => dtp.ThanhPhanDiem?.Ten == "Giữa kỳ")?.Diem,
                    DiemCuoiKy = diemThanhPhanList.FirstOrDefault(dtp => dtp.ThanhPhanDiem?.Ten == "Cuối kỳ")?.Diem
                };
                return Ok(new ApiResponseDTO<BangDiemChiTietDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 12. GET: /api/sinhvien/lop-hoc/{id}/tai-lieu
        [HttpGet("{id}/tai-lieu")]
        public async Task<IActionResult> GetDanhSachTaiLieu(int id)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy danh sách tài liệu của lớp
                var taiLieuList = await _context.BaiHocLops
                    .Include(bl => bl.BaiHoc)
                        .ThenInclude(b => b.CreatedByNavigation)
                    .Include(bl => bl.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(bl => bl.BaiHoc)
                        .ThenInclude(b => b.Files)
                    .Where(bl => bl.LopHocId == id)
                    .OrderByDescending(bl => bl.BaiHoc.CreatedAt)
                    .ToListAsync();
                var danhSachTaiLieu = new List<TaiLieuItemDTO>();
                foreach (var baiHocLop in taiLieuList)
                {
                    var baiHoc = baiHocLop.BaiHoc;
                    var file = baiHoc?.Files?.FirstOrDefault();
                    if (file != null)
                    {
                        var taiLieu = new TaiLieuItemDTO
                        {
                            TaiLieuId = baiHoc.BaiHocId,
                            TieuDe = baiHoc.TieuDe,
                            Loai = baiHoc.LoaiBaiHoc,
                            MonHoc = baiHocLop.LopHoc?.MonHoc?.TenMon,
                            ThoiGianDang = baiHoc.CreatedAt?.ToString("dd/MM/yyyy HH:mm"),
                            GiangVien = baiHoc.CreatedByNavigation?.HoTen,
                            DuongDan = file.DuongDan,
                            KichThuoc = file.KichThuoc,
                            NguoiDang = baiHoc.CreatedByNavigation?.HoTen
                        };
                        danhSachTaiLieu.Add(taiLieu);
                    }
                }
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Data = new { TaiLieu = danhSachTaiLieu }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // 13. GET: /api/sinhvien/lop-hoc/{id}/tai-lieu/{fileId}/download
        [HttpGet("{id}/tai-lieu/{fileId}/download")]
        public async Task<IActionResult> DownloadTaiLieu(int id, int fileId)
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }
                // Kiểm tra sinh viên có thuộc lớp này không
                if (!await KiemTraSinhVienThuocLop(sinhVienId.Value, id))
                {
                    return Forbid();
                }
                // Lấy thông tin file
                var file = await _context.Files
                    .Include(f => f.BaiHoc)
                        .ThenInclude(b => b.BaiHocLops)
                    .FirstOrDefaultAsync(f => f.FilesId == fileId);
                if (file == null || file.BaiHoc == null || !file.BaiHoc.BaiHocLops.Any(bl => bl.LopHocId == id))
                {
                    return NotFound(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy tài liệu"
                    });
                }
                // Trong thực tế, trả về file từ server (ví dụ: sử dụng FileStreamResult)
                // Ví dụ: var fileStream = new FileStream(file.DuongDan, FileMode.Open);
                // return File(fileStream, "application/octet-stream", file.TenFile);
                // Ở đây chỉ trả về thông tin demo
                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Data = new
                    {
                        FileName = file.TenFile,
                        FilePath = file.DuongDan,
                        FileSize = file.KichThuoc,
                        Message = "Trong môi trường production, đây sẽ là file download link"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }



        }

        // 14. GET: /api/sinhvien/lop-hoc/bai-tap
        [HttpGet("bai-tap")]
        public async Task<IActionResult> GetAllBaiTap()
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Lấy tất cả lớp học của sinh viên
                var lopHocIds = await _context.SinhVienLops
                    .Where(svl => svl.SinhVienId == sinhVienId)
                    .Select(svl => svl.LopHocId)
                    .ToListAsync();

                // Lấy tất cả bài tập/kiểm tra từ các lớp của sinh viên
                var baiTapList = await _context.BaiKiemTras
                    .Include(b => b.MonHoc)
                    .Include(b => b.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                            .ThenInclude(g => g.NguoiDung)
                    .Where(b => lopHocIds.Contains(b.LopHocId))
                    .OrderByDescending(b => b.NgayKetThuc)
                    .ToListAsync();

                // Lấy danh sách bài nộp của sinh viên
                var baiNopList = await _context.BaiNops
                    .Where(bn => bn.SinhVienId == sinhVienId)
                    .Include(bn => bn.BaiKiemTra)
                    .ToListAsync();

                var danhSachBaiTap = new List<BaiTapItemDTO>();
                int soBaiDaNop = 0;
                int soBaiChuaNop = 0;

                foreach (var baiTap in baiTapList)
                {
                    var baiNop = baiNopList.FirstOrDefault(bn => bn.BaiKiemTraId == baiTap.BaiKiemTraId);
                    bool daNop = baiNop != null;

                    if (daNop) soBaiDaNop++;
                    else soBaiChuaNop++;

                    string? giangVienName = "Chưa có giảng viên";
                    if (baiTap.GiangVienId.HasValue)
                    {
                        var giangVien = await _context.GiangViens
                            .Include(g => g.NguoiDung)
                            .FirstOrDefaultAsync(g => g.GiangVienId == baiTap.GiangVienId.Value);
                        giangVienName = giangVien?.NguoiDung?.HoTen;
                    }

                    var baiTapItem = new BaiTapItemDTO
                    {
                        BaiTapId = baiTap.BaiKiemTraId,
                        TieuDe = baiTap.TieuDe,
                        MonHoc = baiTap.MonHoc?.TenMon,
                        LopHoc = baiTap.LopHoc?.TenLop,
                        GiangVien = giangVienName,
                        ThoiGianNop = baiTap.NgayKetThuc?.ToString("dd/MM/yyyy HH:mm"),
                        TrangThai = daNop ? "Đã nộp" : "Chưa làm",
                        LoaiBai = baiTap.Loai,
                        DiemToiDa = baiTap.DiemToiDa,
                        DiemDatDuoc = daNop ? baiNop?.TongDiem : null,
                        ThoiGianNopBai = daNop ? baiNop?.NgayNop?.ToString("dd/MM/yyyy HH:mm") : null,
                        ThoiGianLamBai = daNop ? baiNop?.ThoiGianHoanThanh : null,
                        ThoiGianLamBaiToiDa = baiTap.ThoiGianLamBai,
                        ChoPhepLamLai = baiTap.ChoPhepLamLai.GetValueOrDefault()
                    };

                    danhSachBaiTap.Add(baiTapItem);
                }

                var response = new AllBaiTapResponseDTO
                {
                    BaiTap = danhSachBaiTap,
                    TongSoBaiTap = danhSachBaiTap.Count,
                    SoBaiDaNop = soBaiDaNop,
                    SoBaiChuaNop = soBaiChuaNop
                };

                return Ok(new ApiResponseDTO<AllBaiTapResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }


        // 15. GET: /api/sinhvien/lop-hoc/bang-diem
        [HttpGet("bang-diem")]
        public async Task<IActionResult> GetAllBangDiem()
        {
            try
            {
                var sinhVienId = await GetSinhVienId();
                if (sinhVienId == null)
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin sinh viên"
                    });
                }

                // Lấy tất cả điểm của sinh viên từ tất cả các lớp
                var diemList = await _context.Diems
                    .Include(d => d.LopHoc)
                        .ThenInclude(l => l.MonHoc)
                    .Include(d => d.HocKy)
                    .Where(d => d.SinhVienId == sinhVienId)
                    .OrderByDescending(d => d.HocKyId)
                    .ThenBy(d => d.LopHoc.MonHoc.MaMon)
                    .ToListAsync();

                var bangDiem = diemList.Select(d => new BangDiemItemDTO
                {
                    LopHocId = d.LopHocId,
                    MaMon = d.LopHoc?.MonHoc?.MaMon,
                    TenMon = d.LopHoc?.MonHoc?.TenMon,
                    HocKy = $"{d.HocKy?.KiHoc} {d.HocKy?.NamHoc}",
                    SoTinChi = d.SoTinChi,
                    DiemTrungBinh = d.DiemTrungBinhMon,
                    GpaMon = d.Gpamon,
                    DiemChu = d.DiemChu,
                    TrangThai = GetTrangThaiMonHoc(d.TrangThai),
                    HeSo = d.HeSoMon
                }).ToList();

                // Tính GPA tổng nếu cần
                decimal? tongGpa = null;
                int? tongTinChi = 0;
                decimal? tongDiemTrungBinh = 0;

                if (bangDiem.Any())
                {
                    tongTinChi = bangDiem.Sum(b => b.SoTinChi ?? 0);
                    tongDiemTrungBinh = bangDiem.Average(b => b.DiemTrungBinh ?? 0);

                    // Tính GPA theo trọng số tín chỉ
                    decimal tongDiem = 0;
                    int tongTinChiGpa = 0;

                    foreach (var diem in bangDiem)
                    {
                        if (diem.GpaMon.HasValue && diem.SoTinChi.HasValue)
                        {
                            tongDiem += diem.GpaMon.Value * diem.SoTinChi.Value;
                            tongTinChiGpa += diem.SoTinChi.Value;
                        }
                    }

                    tongGpa = tongTinChiGpa > 0 ? Math.Round(tongDiem / tongTinChiGpa, 2) : 0;
                }

                var response = new AllBangDiemResponseDTO
                {
                    BangDiem = bangDiem,
                    TongSoMon = bangDiem.Count,
                    TongTinChi = tongTinChi,
                    GpaTong = tongGpa,
                    DiemTrungBinhTong = Math.Round(tongDiemTrungBinh ?? 0, 2)
                };

                return Ok(new ApiResponseDTO<AllBangDiemResponseDTO>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }




}