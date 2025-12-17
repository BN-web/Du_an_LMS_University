using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LMS_GV.Models.Data;
using LMS_GV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/thong-bao")]
    [Authorize(Roles = "Admin")]
    public class ThongBaoController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ThongBaoController(AppDbContext db)
        {
            _db = db;
        }

        public class ThongBaoListQuery
        {
            public string? Target { get; set; } // "sinh-vien" | "giang-vien"
            public int? GroupId { get; set; }   // không dùng triệt để do thiếu cột
            public string? Q { get; set; }
        }

        public class CreateThongBaoRequest
        {
            [Required]
            public string TieuDe { get; set; } = string.Empty;

            public string? NoiDung { get; set; }

            [Required]
            public int NguoiDangId { get; set; }

            public string? Target { get; set; }  // "all" | "khoa" | "nganh" | "lop-hoc" | "sinh-vien" | "giang-vien"
            public int? GroupId { get; set; }    // Id của nhóm tương ứng
        }

        public class UpdateThongBaoRequest
        {
            [Required]
            public string TieuDe { get; set; } = string.Empty;

            public string? NoiDung { get; set; }
        }

        public class PublishRequest
        {
            [Required]
            public string Target { get; set; } = "all";
            public int? GroupId { get; set; }
        }

        // helper: build người nhận theo target/groupId
        private async Task<List<int>> ResolveReceivers(string target, int? groupId)
        {
            target = target.ToLower().Trim();
            var result = new List<int>();

            if (target == "all")
            {
                result = await _db.NguoiDungs
                    .Select(x => x.NguoiDungId)
                    .ToListAsync();
            }
            else if (target == "sinh-vien")
            {
                result = await (
                    from sv in _db.HoSoSinhViens
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    where nd.VaiTroId == 4
                    select nd.NguoiDungId
                ).ToListAsync();
            }
            else if (target == "giang-vien")
            {
                result = await (
                    from gv in _db.GiangViens
                    join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                    where nd.VaiTroId == 2 || nd.VaiTroId == 3
                    select nd.NguoiDungId
                ).ToListAsync();
            }
            else if (target == "khoa" && groupId.HasValue)
            {
                int khoaId = groupId.Value;

                var svNguoiDung = await (
                    from sv in _db.HoSoSinhViens
                    join ng in _db.Nganhs on sv.NganhId equals ng.NganhId
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    where ng.KhoaId == khoaId
                    select nd.NguoiDungId
                ).ToListAsync();

                var gvNguoiDung = await (
                    from gv in _db.GiangViens
                    join nd in _db.NguoiDungs on gv.NguoiDungId equals nd.NguoiDungId
                    where gv.KhoaId == khoaId
                    select nd.NguoiDungId
                ).ToListAsync();

                result = svNguoiDung.Concat(gvNguoiDung).Distinct().ToList();
            }
            else if (target == "nganh" && groupId.HasValue)
            {
                int nganhId = groupId.Value;

                result = await (
                    from sv in _db.HoSoSinhViens
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    where sv.NganhId == nganhId
                    select nd.NguoiDungId
                ).ToListAsync();
            }
            else if (target == "lop-hoc" && groupId.HasValue)
            {
                int lopHocId = groupId.Value;

                result = await (
                    from svlop in _db.SinhVienLops
                    join sv in _db.HoSoSinhViens on svlop.SinhVienId equals sv.SinhVienId
                    join nd in _db.NguoiDungs on sv.NguoiDungId equals nd.NguoiDungId
                    where svlop.LopHocId == lopHocId
                    select nd.NguoiDungId
                ).ToListAsync();
            }

            return result.Distinct().ToList();
        }

        // 1. GET /
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] ThongBaoListQuery queryModel)
        {
            var query = _db.ThongBaos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Q))
            {
                var kw = queryModel.Q.Trim();
                query = query.Where(x =>
                    (x.TieuDe ?? "").Contains(kw) ||
                    (x.NoiDung ?? "").Contains(kw));
            }

            var listRaw = await (
                from tb in query
                join nd in _db.NguoiDungs on tb.NguoiDangId equals nd.NguoiDungId into gnd
                from nd in gnd.DefaultIfEmpty()
                select new
                {
                    tb,
                    nguoiDang = nd
                }
            ).OrderByDescending(x => x.tb.NgayTao)
             .ToListAsync();

            // tính "đối tượng" dựa trên người nhận
            var result = new List<object>();

            foreach (var item in listRaw)
            {
                var receivers = await (
                    from tbn in _db.ThongBaoNguoiDungs.AsNoTracking()
                    join nd in _db.NguoiDungs.AsNoTracking()
                        on tbn.NguoiDungId equals nd.NguoiDungId
                    where tbn.ThongBaoId == item.tb.ThongBaoId
                    select nd.VaiTroId
                ).ToListAsync();

                string doiTuong;
                if (receivers.Count == 0) doiTuong = "Không xác định";
                else if (receivers.All(v => v == 4)) doiTuong = "Sinh viên";
                else if (receivers.All(v => v == 2 || v == 3)) doiTuong = "Giảng viên";
                else doiTuong = "Nhiều đối tượng";

                // filter target sơ bộ
                if (!string.IsNullOrWhiteSpace(queryModel.Target))
                {
                    var t = queryModel.Target.ToLower();
                    if (t == "sinh-vien" && doiTuong != "Sinh viên") continue;
                    if (t == "giang-vien" && doiTuong != "Giảng viên") continue;
                }

                result.Add(new
                {
                    id = item.tb.ThongBaoId,
                    tieuDe = item.tb.TieuDe,
                    doiTuong,
                    ngayTao = item.tb.NgayTao,
                    nguoiDang = item.nguoiDang?.HoTen
                });
            }

            return Ok(result);
        }

        // 2. GET /{id} – detail
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var tb = await _db.ThongBaos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ThongBaoId == id);
            if (tb == null)
                return NotFound(new { message = "Không tìm thấy thông báo" });

            var nguoiDang = await _db.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NguoiDungId == tb.NguoiDangId);

            var nguoiNhan = await (
                from tbn in _db.ThongBaoNguoiDungs.AsNoTracking()
                join nd in _db.NguoiDungs.AsNoTracking()
                    on tbn.NguoiDungId equals nd.NguoiDungId
                select new
                {
                    id = tbn.ThongBaoNguoiDungId,
                    nguoiDungId = nd.NguoiDungId,
                    hoTen = nd.HoTen,
                    email = nd.Email,
                    daDoc = tbn.DaDoc,
                    ngayNhan = tbn.CreatedAt
                }
            ).ToListAsync();

            var result = new
            {
                id = tb.ThongBaoId,
                tieuDe = tb.TieuDe,
                noiDung = tb.NoiDung,
                nguoiGui = nguoiDang != null ? new { nguoiDang.HoTen, nguoiDang.Email } : null,
                ngayTao = tb.NgayTao,
                nguoiNhan
            };

            return Ok(result);
        }

        // 3. POST /
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateThongBaoRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ndExists = await _db.NguoiDungs
                .AnyAsync(x => x.NguoiDungId == req.NguoiDangId);
            if (!ndExists)
                return BadRequest(new { field = "nguoiDangId", message = "Người đăng không tồn tại" });

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

            // Nếu truyền target/groupId thì publish luôn
            if (!string.IsNullOrWhiteSpace(req.Target))
            {
                var receivers = await ResolveReceivers(req.Target, req.GroupId);
                foreach (var ndId in receivers)
                {
                    var entity = new ThongBaoNguoiDung
                    {
                        ThongBaoId = tb.ThongBaoId,
                        NguoiDungId = ndId,
                        DaDoc = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.ThongBaoNguoiDungs.Add(entity);
                }

                await _db.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetDetail), new { id = tb.ThongBaoId }, new
            {
                id = tb.ThongBaoId
            });
        }

        // 4. PUT /{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateThongBaoRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tb = await _db.ThongBaos
                .FirstOrDefaultAsync(x => x.ThongBaoId == id);
            if (tb == null)
                return NotFound(new { message = "Không tìm thấy thông báo" });

            tb.TieuDe = req.TieuDe;
            tb.NoiDung = req.NoiDung;
            tb.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 5. DELETE /{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tb = await _db.ThongBaos
                .FirstOrDefaultAsync(x => x.ThongBaoId == id);
            if (tb == null)
                return NotFound(new { message = "Không tìm thấy thông báo" });

            var receivers = _db.ThongBaoNguoiDungs
                .Where(x => x.ThongBaoId == id);
            _db.ThongBaoNguoiDungs.RemoveRange(receivers);

            _db.ThongBaos.Remove(tb);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 6. POST /{id}/publish
        [HttpPost("{id:int}/publish")]
        public async Task<IActionResult> Publish(int id, [FromBody] PublishRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tb = await _db.ThongBaos
                .FirstOrDefaultAsync(x => x.ThongBaoId == id);
            if (tb == null)
                return NotFound(new { message = "Không tìm thấy thông báo" });

            var receivers = await ResolveReceivers(req.Target, req.GroupId);
            if (receivers.Count == 0)
                return BadRequest(new { message = "Không tìm thấy người nhận phù hợp" });

            foreach (var ndId in receivers)
            {
                var exists = await _db.ThongBaoNguoiDungs
                    .AnyAsync(x => x.ThongBaoId == id && x.NguoiDungId == ndId);
                if (exists) continue;

                var entity = new ThongBaoNguoiDung
                {
                    ThongBaoId = id,
                    NguoiDungId = ndId,
                    DaDoc = false,
                    CreatedAt = DateTime.UtcNow
                };
                _db.ThongBaoNguoiDungs.Add(entity);
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã phát hành thông báo" });
        }
    }
}
