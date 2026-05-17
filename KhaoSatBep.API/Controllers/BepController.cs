using KhaoSatBep.API.Data;
using KhaoSatBep.API.DTOs;
using KhaoSatBep.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhaoSatBep.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BepController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BepController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách tất cả bếp</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var beps = await _context.Beps
                .Include(b => b.KhaoSats)
                .Select(b => new BepDto
                {
                    Id = b.Id,
                    TenBep = b.TenBep,
                    DiaChi = b.DiaChi,
                    LoaiBep = b.LoaiBep,
                    MoTa = b.MoTa,
                    NgayTao = b.NgayTao,
                    SoLuongKhaoSat = b.KhaoSats.Count
                })
                .ToListAsync();

            return Ok(beps);
        }

        /// <summary>Lấy chi tiết một bếp theo ID</summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var bep = await _context.Beps
                .Include(b => b.KhaoSats)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bep == null)
                return NotFound(new { message = "Không tìm thấy bếp" });

            return Ok(new BepDto
            {
                Id = bep.Id,
                TenBep = bep.TenBep,
                DiaChi = bep.DiaChi,
                LoaiBep = bep.LoaiBep,
                MoTa = bep.MoTa,
                NgayTao = bep.NgayTao,
                SoLuongKhaoSat = bep.KhaoSats.Count
            });
        }

        /// <summary>Thêm bếp mới</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaoBepDto dto)
        {
            var bep = new Bep
            {
                TenBep = dto.TenBep,
                DiaChi = dto.DiaChi,
                LoaiBep = dto.LoaiBep,
                MoTa = dto.MoTa
            };

            _context.Beps.Add(bep);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = bep.Id }, new { bep.Id, bep.TenBep });
        }

        /// <summary>Cập nhật thông tin bếp</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaoBepDto dto)
        {
            var bep = await _context.Beps.FindAsync(id);
            if (bep == null)
                return NotFound(new { message = "Không tìm thấy bếp" });

            bep.TenBep = dto.TenBep;
            bep.DiaChi = dto.DiaChi;
            bep.LoaiBep = dto.LoaiBep;
            bep.MoTa = dto.MoTa;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Xóa bếp (chỉ Admin)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var bep = await _context.Beps.FindAsync(id);
            if (bep == null)
                return NotFound(new { message = "Không tìm thấy bếp" });

            _context.Beps.Remove(bep);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
