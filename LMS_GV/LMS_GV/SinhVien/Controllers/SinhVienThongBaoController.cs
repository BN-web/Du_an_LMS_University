using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_GV.Models.Data;
using System.Security.Claims;
using LMS_GV.Models;

namespace LMS_GV.Controllers.SinhVien
{
    [Route("api/sinhvien/thong-bao")]
    [ApiController]
    public class SinhVienThongBaoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SinhVienThongBaoController> _logger;

        public SinhVienThongBaoController(AppDbContext context, ILogger<SinhVienThongBaoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("nameid")?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("UserId")?.Value
                            ?? User.FindFirst("NguoiDung_id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng");
            }

            return userId;
        }

        // GET: api/sinhvien/thong-bao
        [HttpGet]
        public async Task<ActionResult<object>> GetThongBaoList(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] bool? daDoc = null) // Bỏ tham số loai
        {
            try
            {
                var userId = GetUserIdFromToken();

                var query = from tb in _context.ThongBaos
                            join tbnd in _context.ThongBaoNguoiDungs
                                .Where(x => x.NguoiDungId == userId)
                                .DefaultIfEmpty()
                            on tb.ThongBaoId equals tbnd.ThongBaoId
                            select new
                            {
                                ThongBao = tb,
                                DaDoc = tbnd != null ? (tbnd.DaDoc == true) : false
                            };

                if (daDoc.HasValue)
                {
                    query = query.Where(x => x.DaDoc == daDoc.Value);
                }

                var orderedQuery = query.OrderByDescending(x => x.ThongBao.NgayTao);
                var total = await orderedQuery.CountAsync();

                var thongBaos = await orderedQuery
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var thongBaoList = thongBaos.Select(x => new
                {
                    id = x.ThongBao.ThongBaoId,
                    tieuDe = x.ThongBao.TieuDe,
                    ngayGui = x.ThongBao.NgayTao.HasValue
                        ? x.ThongBao.NgayTao.Value.ToString("dd/MM/yyyy HH:mm")
                        : string.Empty,
                    noiDungRutGon = !string.IsNullOrEmpty(x.ThongBao.NoiDung)
                        ? (x.ThongBao.NoiDung.Length > 100 ? x.ThongBao.NoiDung.Substring(0, 100) + "..." : x.ThongBao.NoiDung)
                        : string.Empty,
                    daDoc = x.DaDoc,
                    coTheXemChiTiet = true
                }).ToList();

                var response = new
                {
                    thongBao = thongBaoList,
                    total = total,
                    page = page,
                    limit = limit,
                    totalPages = (int)Math.Ceiling(total / (double)limit)
                };

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi lấy danh sách thông báo",
                    error = ex.Message
                });
            }
        }

        // GET: api/sinhvien/thong-bao/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetThongBaoDetail(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();

                var thongBao = await _context.ThongBaos
                    .Include(tb => tb.NguoiDang)
                    .FirstOrDefaultAsync(tb => tb.ThongBaoId == id);

                if (thongBao == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông báo"
                    });
                }

                var thongBaoNguoiDung = await _context.ThongBaoNguoiDungs
                    .FirstOrDefaultAsync(tbnd => tbnd.ThongBaoId == id && tbnd.NguoiDungId == userId);

                if (thongBaoNguoiDung == null)
                {
                    thongBaoNguoiDung = new ThongBaoNguoiDung
                    {
                        ThongBaoId = id,
                        NguoiDungId = userId,
                        DaDoc = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.ThongBaoNguoiDungs.Add(thongBaoNguoiDung);
                }
                else if (thongBaoNguoiDung.DaDoc != true)
                {
                    thongBaoNguoiDung.DaDoc = true;
                    _context.ThongBaoNguoiDungs.Update(thongBaoNguoiDung);
                }

                await _context.SaveChangesAsync();

                var response = new
                {
                    id = thongBao.ThongBaoId,
                    tieuDe = thongBao.TieuDe,
                    ngayGui = thongBao.NgayTao.HasValue
                        ? thongBao.NgayTao.Value.ToString("dd/MM/yyyy HH:mm")
                        : string.Empty,
                    noiDung = thongBao.NoiDung ?? string.Empty,
                    daDoc = true,
                    nguoiGui = thongBao.NguoiDang?.HoTen ?? "Hệ thống"
                    // Bỏ loaiThongBao
                };

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi lấy chi tiết thông báo",
                    error = ex.Message
                });
            }
        }

        // PUT: api/sinhvien/thong-bao/{id}/da-doc
        [HttpPut("{id}/da-doc")]
        public async Task<ActionResult<object>> MarkAsRead(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();

                var thongBao = await _context.ThongBaos
                    .FirstOrDefaultAsync(tb => tb.ThongBaoId == id);

                if (thongBao == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông báo"
                    });
                }

                var thongBaoNguoiDung = await _context.ThongBaoNguoiDungs
                    .FirstOrDefaultAsync(tbnd => tbnd.ThongBaoId == id && tbnd.NguoiDungId == userId);

                if (thongBaoNguoiDung == null)
                {
                    thongBaoNguoiDung = new ThongBaoNguoiDung
                    {
                        ThongBaoId = id,
                        NguoiDungId = userId,
                        DaDoc = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.ThongBaoNguoiDungs.Add(thongBaoNguoiDung);
                }
                else if (thongBaoNguoiDung.DaDoc != true)
                {
                    thongBaoNguoiDung.DaDoc = true;
                    _context.ThongBaoNguoiDungs.Update(thongBaoNguoiDung);
                }
                else
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Thông báo đã được đánh dấu đã đọc từ trước"
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Đã đánh dấu thông báo là đã đọc"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi trong MarkAsRead: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi đánh dấu thông báo",
                    error = ex.Message
                });
            }
        }

        // GET: api/sinhvien/thong-bao/chua-doc
        [HttpGet("chua-doc")]
        public async Task<ActionResult<object>> GetUnreadCount()
        {
            try
            {
                var userId = GetUserIdFromToken();

                var thongBaoChuaDoc = await (from tb in _context.ThongBaos
                                             join tbnd in _context.ThongBaoNguoiDungs
                                                 .Where(x => x.NguoiDungId == userId)
                                                 .DefaultIfEmpty()
                                             on tb.ThongBaoId equals tbnd.ThongBaoId
                                             where tbnd == null || tbnd.DaDoc != true
                                             orderby tb.NgayTao descending
                                             select new
                                             {
                                                 ThongBaoId = tb.ThongBaoId,
                                                 TieuDe = tb.TieuDe,
                                                 NgayTao = tb.NgayTao
                                             }).ToListAsync();

                var count = thongBaoChuaDoc.Count;
                object thongBaoMoiNhat = null;

                if (count > 0)
                {
                    var newest = thongBaoChuaDoc.First();
                    thongBaoMoiNhat = new
                    {
                        id = newest.ThongBaoId,
                        tieuDe = newest.TieuDe,
                        ngayGui = newest.NgayTao.HasValue
                            ? newest.NgayTao.Value.ToString("dd/MM/yyyy HH:mm")
                            : string.Empty
                    };
                }

                var response = new
                {
                    soThongBaoChuaDoc = count,
                    thongBaoMoiNhat = thongBaoMoiNhat
                };

                return Ok(new
                {
                    success = true,
                    data = response
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số thông báo chưa đọc");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi lấy số thông báo chưa đọc"
                });
            }
        }

        // PUT: api/sinhvien/thong-bao/danh-dau-tat-ca-da-doc
        [HttpPut("danh-dau-tat-ca-da-doc")]
        public async Task<ActionResult<object>> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserIdFromToken();

                var thongBaoChuaDoc = await (from tb in _context.ThongBaos
                                             join tbnd in _context.ThongBaoNguoiDungs
                                                 .Where(x => x.NguoiDungId == userId)
                                                 .DefaultIfEmpty()
                                             on tb.ThongBaoId equals tbnd.ThongBaoId
                                             where tbnd == null || tbnd.DaDoc != true
                                             select new
                                             {
                                                 ThongBaoId = tb.ThongBaoId,
                                                 ExistingRecord = tbnd
                                             }).ToListAsync();

                var now = DateTime.Now;
                foreach (var item in thongBaoChuaDoc)
                {
                    if (item.ExistingRecord == null)
                    {
                        var newRecord = new ThongBaoNguoiDung
                        {
                            ThongBaoId = item.ThongBaoId,
                            NguoiDungId = userId,
                            DaDoc = true,
                            CreatedAt = now
                        };
                        _context.ThongBaoNguoiDungs.Add(newRecord);
                    }
                    else
                    {
                        item.ExistingRecord.DaDoc = true;
                        _context.ThongBaoNguoiDungs.Update(item.ExistingRecord);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Đã đánh dấu {thongBaoChuaDoc.Count} thông báo là đã đọc"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu tất cả thông báo là đã đọc");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi đánh dấu thông báo"
                });
            }
        }
    }
}