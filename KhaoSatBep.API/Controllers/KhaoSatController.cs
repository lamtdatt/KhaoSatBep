using KhaoSatBep.API.Data;
using KhaoSatBep.API.DTOs;
using KhaoSatBep.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KhaoSatBep.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KhaoSatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KhaoSatController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Lấy danh sách khảo sát</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserId();

            var query = _context.KhaoSats
                .Include(k => k.Bep)
                .Include(k => k.NguoiKhaoSat)
                .Include(k => k.ChiTiets)
                .AsQueryable();

            // NguoiDung chỉ thấy khảo sát của mình
            if (!isAdmin)
                query = query.Where(k => k.NguoiKhaoSatId == userId);

            var ksats = await query
                .OrderByDescending(k => k.NgayTao)
                .Select(k => new KhaoSatDto
                {
                    Id = k.Id,
                    TieuDe = k.TieuDe,
                    NgayKhaoSat = k.NgayKhaoSat,
                    TrangThai = k.TrangThai,
                    GhiChu = k.GhiChu,
                    NgayTao = k.NgayTao,
                    TenBep = k.Bep.TenBep,
                    NguoiKhaoSat = k.NguoiKhaoSat.HoTen,
                    DiemTrungBinh = k.ChiTiets.Count > 0 ? k.ChiTiets.Average(c => c.Diem) : 0
                })
                .ToListAsync();

            return Ok(ksats);
        }

        /// <summary>Lấy chi tiết khảo sát</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserId();

            var ksat = await _context.KhaoSats
                .Include(k => k.Bep)
                .Include(k => k.NguoiKhaoSat)
                .Include(k => k.ChiTiets)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (ksat == null)
                return NotFound(new { message = "Không tìm thấy khảo sát" });

            if (!isAdmin && ksat.NguoiKhaoSatId != userId)
                return Forbid();

            return Ok(new
            {
                ksat.Id,
                ksat.TieuDe,
                ksat.NgayKhaoSat,
                ksat.TrangThai,
                ksat.GhiChu,
                ksat.NgayTao,
                TenBep = ksat.Bep.TenBep,
                DiaChi = ksat.Bep.DiaChi,
                NguoiKhaoSat = ksat.NguoiKhaoSat.HoTen,
                DiemTrungBinh = ksat.ChiTiets.Count > 0 ? ksat.ChiTiets.Average(c => c.Diem) : 0,
                ChiTiets = ksat.ChiTiets.Select(c => new ChiTietDto
                {
                    Id = c.Id,
                    HangMuc = c.HangMuc,
                    Diem = c.Diem,
                    NhanXet = c.NhanXet
                })
            });
        }

        /// <summary>Tạo khảo sát mới</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaoKhaoSatDto dto)
        {
            var bep = await _context.Beps.FindAsync(dto.BepId);
            if (bep == null)
                return BadRequest(new { message = "Bếp không tồn tại" });

            var ksat = new KhaoSat
            {
                TieuDe = dto.TieuDe,
                NgayKhaoSat = dto.NgayKhaoSat,
                GhiChu = dto.GhiChu,
                BepId = dto.BepId,
                NguoiKhaoSatId = GetUserId(),
                TrangThai = "ChoDuyet"
            };

            foreach (var ct in dto.ChiTiets)
            {
                ksat.ChiTiets.Add(new ChiTietKhaoSat
                {
                    HangMuc = ct.HangMuc,
                    Diem = ct.Diem,
                    NhanXet = ct.NhanXet
                });
            }

            _context.KhaoSats.Add(ksat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ksat.Id }, new { ksat.Id, ksat.TieuDe });
        }

        /// <summary>Cập nhật trạng thái khảo sát (Admin)</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CapNhatKhaoSatDto dto)
        {
            var isAdmin = User.IsInRole("Admin");
            var userId = GetUserId();

            var ksat = await _context.KhaoSats.FindAsync(id);
            if (ksat == null)
                return NotFound(new { message = "Không tìm thấy khảo sát" });

            if (!isAdmin && ksat.NguoiKhaoSatId != userId)
                return Forbid();

            ksat.TieuDe = dto.TieuDe;
            ksat.NgayKhaoSat = dto.NgayKhaoSat;
            ksat.GhiChu = dto.GhiChu;

            // Chỉ Admin mới được đổi trạng thái
            if (isAdmin)
                ksat.TrangThai = dto.TrangThai;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Xóa khảo sát (Admin)</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ksat = await _context.KhaoSats.FindAsync(id);
            if (ksat == null)
                return NotFound(new { message = "Không tìm thấy khảo sát" });

            _context.KhaoSats.Remove(ksat);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Thống kê tổng quan (Admin)</summary>
        [HttpGet("thong-ke")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ThongKe()
        {
            var tongKhaoSat = await _context.KhaoSats.CountAsync();
            var choDuyet = await _context.KhaoSats.CountAsync(k => k.TrangThai == "ChoDuyet");
            var daDuyet = await _context.KhaoSats.CountAsync(k => k.TrangThai == "DaDuyet");
            var tongBep = await _context.Beps.CountAsync();
            var tongNguoiDung = await _context.Users.CountAsync();

            return Ok(new
            {
                TongKhaoSat = tongKhaoSat,
                ChoDuyet = choDuyet,
                DaDuyet = daDuyet,
                TuChoi = tongKhaoSat - choDuyet - daDuyet,
                TongBep = tongBep,
                TongNguoiDung = tongNguoiDung
            });
        }
    }
}
