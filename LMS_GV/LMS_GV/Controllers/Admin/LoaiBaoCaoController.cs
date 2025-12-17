using System.Threading.Tasks;
using LMS_GV.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_GV.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/loai-bao-cao")]
    [Authorize(Roles = "Admin")]
    public class LoaiBaoCaoController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LoaiBaoCaoController(AppDbContext db)
        {
            _db = db;
        }

        // 1. GET /
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var list = await _db.LoaiBaoCaos
                .AsNoTracking()
                .OrderBy(x => x.TenLoai)
                .Select(x => new
                {
                    id = x.LoaiBaoCaoId,
                    tenLoai = x.TenLoai,
                    moTa = x.MoTa
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
