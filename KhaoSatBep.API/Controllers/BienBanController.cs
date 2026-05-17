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
    public class BienBanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BienBanController(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // GET: api/BienBan
        // Lấy danh sách biên bản (có filter)
        // ============================
        [HttpGet]
        public async Task<ActionResult<List<BienBanSummaryDto>>> GetAll(
            [FromQuery] string? loai,
            [FromQuery] string? trangThai,
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            var query = _context.BienBans
                .Include(b => b.NguoiTao)
                .Include(b => b.ChiTiets)
                .AsQueryable();

            // Employee chỉ thấy BB của mình, Admin thấy tất cả
            if (userRole != "Admin")
                query = query.Where(b => b.NguoiTaoId == userId);

            // Filters
            if (!string.IsNullOrEmpty(loai))
                query = query.Where(b => b.LoaiBienBan == loai);

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(b => b.TrangThai == trangThai);

            if (tuNgay.HasValue)
                query = query.Where(b => b.NgayKiemTra >= tuNgay.Value);

            if (denNgay.HasValue)
                query = query.Where(b => b.NgayKiemTra <= denNgay.Value);

            var result = await query
                .OrderByDescending(b => b.NgayTao)
                .Select(b => new BienBanSummaryDto
                {
                    Id = b.Id,
                    SoBienBan = b.SoBienBan,
                    LoaiBienBan = b.LoaiBienBan,
                    NgayKiemTra = b.NgayKiemTra,
                    TrangThai = b.TrangThai,
                    NguoiTao = b.NguoiTao.HoTen,
                    NgayTao = b.NgayTao,
                    SoMucDat = b.ChiTiets.Count(c => c.Dat == true),
                    SoMucKhongDat = b.ChiTiets.Count(c => c.Dat == false),
                    TongSoMuc = b.ChiTiets.Count()
                })
                .ToListAsync();

            return Ok(result);
        }

        // ============================
        // GET: api/BienBan/5
        // Lấy chi tiết 1 biên bản
        // ============================
        [HttpGet("{id}")]
        public async Task<ActionResult<BienBanChiTietDto>> GetById(int id)
        {
            var bienBan = await _context.BienBans
                .Include(b => b.NguoiTao)
                .Include(b => b.ThanhPhans)
                .Include(b => b.ChiTiets)
                .Include(b => b.DinhLuongs)
                .Include(b => b.ChuKys)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bienBan == null)
                return NotFound(new { message = "Không tìm thấy biên bản" });

            // Kiểm tra quyền: Employee chỉ xem BB của mình
            var userId = GetUserId();
            var userRole = GetUserRole();
            if (userRole != "Admin" && bienBan.NguoiTaoId != userId)
                return Forbid();

            var result = new BienBanChiTietDto
            {
                Id = bienBan.Id,
                SoBienBan = bienBan.SoBienBan,
                LoaiBienBan = bienBan.LoaiBienBan,
                NgayKiemTra = bienBan.NgayKiemTra,
                TrangThai = bienBan.TrangThai,
                GopYKhoaDinhDuong = bienBan.GopYKhoaDinhDuong,
                YKienBPCB = bienBan.YKienBPCB,
                NguoiTao = bienBan.NguoiTao.HoTen,
                NgayTao = bienBan.NgayTao,
                BuaAnDuongMieng = bienBan.BuaAnDuongMieng,
                ThucDonHangNgay = bienBan.ThucDonHangNgay,
                BuaAnOngThong = bienBan.BuaAnOngThong,
                ThanhPhans = bienBan.ThanhPhans
                    .OrderBy(t => t.STT)
                    .Select(t => new ThanhPhanDto { STT = t.STT, HoTen = t.HoTen, ChucVu = t.ChucVu })
                    .ToList(),
                ChiTiets = bienBan.ChiTiets
                    .OrderBy(c => c.MucSo)
                    .Select(c => new ChiTietBienBanDto
                    {
                        Id = c.Id,
                        MucSo = c.MucSo,
                        PhanNhom = c.PhanNhom,
                        NoiDung = c.NoiDung,
                        Dat = c.Dat,
                        GhiChu = c.GhiChu
                    })
                    .ToList(),
                ChuKys = bienBan.ChuKys
                    .Select(c => new ChuKyDto
                    {
                        ViTri = c.ViTri,
                        TenNguoiKy = c.TenNguoiKy,
                        ChucVuNguoiKy = c.ChucVuNguoiKy,
                        DuLieuChuKy = c.DuLieuChuKy
                    })
                    .ToList(),
                DinhLuongs = bienBan.DinhLuongs.Any() ?
                    bienBan.DinhLuongs
                        .OrderBy(d => d.STT)
                        .Select(d => new DinhLuongDto
                        {
                            STT = d.STT,
                            LoaiSuatAn = d.LoaiSuatAn,
                            NoiDung = d.NoiDung,
                            CheDoAn1Ten = d.CheDoAn1Ten,
                            CheDoAn1KhoiLuong = d.CheDoAn1KhoiLuong,
                            CheDoAn1Dat = d.CheDoAn1Dat,
                            CheDoAn1KhongDat = d.CheDoAn1KhongDat,
                            CheDoAn2Ten = d.CheDoAn2Ten,
                            CheDoAn2KhoiLuong = d.CheDoAn2KhoiLuong,
                            CheDoAn2Dat = d.CheDoAn2Dat,
                            CheDoAn2KhongDat = d.CheDoAn2KhongDat
                        })
                        .ToList()
                    : null
            };

            return Ok(result);
        }

        // ============================
        // POST: api/BienBan
        // Tạo biên bản mới
        // ============================
        [HttpPost]
        public async Task<ActionResult<BienBanSummaryDto>> Create([FromBody] TaoBienBanDto dto)
        {
            try
            {
            var userId = GetUserId();

            // Validate loại biên bản
            var loaiHopLe = new[] { "CSHT", "HoSo", "VeSinh", "SuatAn" };
            if (!loaiHopLe.Contains(dto.LoaiBienBan))
                return BadRequest(new { message = "Loại biên bản không hợp lệ. Chấp nhận: CSHT, HoSo, VeSinh, SuatAn" });

            var bienBan = new BienBan
            {
                SoBienBan = dto.SoBienBan,
                LoaiBienBan = dto.LoaiBienBan,
                NgayKiemTra = ToUtc(dto.NgayKiemTra),
                NgayTao = DateTime.UtcNow,
                TrangThai = "ChuaGui",
                GopYKhoaDinhDuong = dto.GopYKhoaDinhDuong,
                YKienBPCB = dto.YKienBPCB,
                NguoiTaoId = userId,
                BuaAnDuongMieng = dto.BuaAnDuongMieng,
                ThucDonHangNgay = dto.ThucDonHangNgay,
                BuaAnOngThong = dto.BuaAnOngThong
            };

            // Thành phần
            foreach (var tp in dto.ThanhPhans)
            {
                bienBan.ThanhPhans.Add(new ThanhPhanKiemTra
                {
                    STT = tp.STT,
                    HoTen = tp.HoTen,
                    ChucVu = tp.ChucVu
                });
            }

            // Chi tiết kiểm tra
            foreach (var ct in dto.ChiTiets)
            {
                bienBan.ChiTiets.Add(new ChiTietBienBan
                {
                    MucSo = ct.MucSo,
                    PhanNhom = ct.PhanNhom,
                    NoiDung = ct.NoiDung,
                    Dat = ct.Dat,
                    GhiChu = ct.GhiChu
                });
            }

            // Chữ ký
            foreach (var ck in dto.ChuKys)
            {
                bienBan.ChuKys.Add(new ChuKy
                {
                    ViTri = ck.ViTri,
                    TenNguoiKy = ck.TenNguoiKy,
                    ChucVuNguoiKy = ck.ChucVuNguoiKy,
                    DuLieuChuKy = ck.DuLieuChuKy
                });
            }

            // Định lượng (BB4)
            if (dto.DinhLuongs != null)
            {
                foreach (var dl in dto.DinhLuongs)
                {
                    bienBan.DinhLuongs.Add(new DinhLuongSuatAn
                    {
                        STT = dl.STT,
                        LoaiSuatAn = dl.LoaiSuatAn,
                        NoiDung = dl.NoiDung,
                        CheDoAn1Ten = dl.CheDoAn1Ten,
                        CheDoAn1KhoiLuong = dl.CheDoAn1KhoiLuong,
                        CheDoAn1Dat = dl.CheDoAn1Dat,
                        CheDoAn1KhongDat = dl.CheDoAn1KhongDat,
                        CheDoAn2Ten = dl.CheDoAn2Ten,
                        CheDoAn2KhoiLuong = dl.CheDoAn2KhoiLuong,
                        CheDoAn2Dat = dl.CheDoAn2Dat,
                        CheDoAn2KhongDat = dl.CheDoAn2KhongDat
                    });
                }
            }

            _context.BienBans.Add(bienBan);
            await _context.SaveChangesAsync();

            // Reload với Include để trả về
            await _context.Entry(bienBan).Reference(b => b.NguoiTao).LoadAsync();
            await _context.Entry(bienBan).Collection(b => b.ChiTiets).LoadAsync();

            var result = new BienBanSummaryDto
            {
                Id = bienBan.Id,
                SoBienBan = bienBan.SoBienBan,
                LoaiBienBan = bienBan.LoaiBienBan,
                NgayKiemTra = bienBan.NgayKiemTra,
                TrangThai = bienBan.TrangThai,
                NguoiTao = bienBan.NguoiTao.HoTen,
                NgayTao = bienBan.NgayTao,
                SoMucDat = bienBan.ChiTiets.Count(c => c.Dat == true),
                SoMucKhongDat = bienBan.ChiTiets.Count(c => c.Dat == false),
                TongSoMuc = bienBan.ChiTiets.Count
            };

            return CreatedAtAction(nameof(GetById), new { id = bienBan.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Khong the tao bien ban",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    detail = ex.InnerException?.InnerException?.Message
                });
            }
        }

        // ============================
        // PUT: api/BienBan/5
        // Cập nhật biên bản (chỉ khi chưa gửi)
        // ============================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaoBienBanDto dto)
        {
            var bienBan = await _context.BienBans
                .Include(b => b.ThanhPhans)
                .Include(b => b.ChiTiets)
                .Include(b => b.DinhLuongs)
                .Include(b => b.ChuKys)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bienBan == null)
                return NotFound(new { message = "Không tìm thấy biên bản" });

            // Chỉ cho sửa khi chưa gửi hoặc bị từ chối
            if (bienBan.TrangThai != "ChuaGui" && bienBan.TrangThai != "TuChoi")
                return BadRequest(new { message = "Không thể sửa biên bản đã gửi hoặc đã duyệt" });

            // Kiểm tra quyền
            var userId = GetUserId();
            if (bienBan.NguoiTaoId != userId)
                return Forbid();

            // Cập nhật header
            bienBan.SoBienBan = dto.SoBienBan;
            bienBan.NgayKiemTra = ToUtc(dto.NgayKiemTra);
            bienBan.GopYKhoaDinhDuong = dto.GopYKhoaDinhDuong;
            bienBan.YKienBPCB = dto.YKienBPCB;
            bienBan.BuaAnDuongMieng = dto.BuaAnDuongMieng;
            bienBan.ThucDonHangNgay = dto.ThucDonHangNgay;
            bienBan.BuaAnOngThong = dto.BuaAnOngThong;
            bienBan.NgayCapNhat = DateTime.UtcNow;

            // Xóa cũ, thêm mới (replace strategy)
            _context.ThanhPhanKiemTras.RemoveRange(bienBan.ThanhPhans);
            _context.ChiTietBienBans.RemoveRange(bienBan.ChiTiets);
            _context.DinhLuongSuatAns.RemoveRange(bienBan.DinhLuongs);
            _context.ChuKys.RemoveRange(bienBan.ChuKys);

            // Thêm lại
            foreach (var tp in dto.ThanhPhans)
                bienBan.ThanhPhans.Add(new ThanhPhanKiemTra { STT = tp.STT, HoTen = tp.HoTen, ChucVu = tp.ChucVu });

            foreach (var ct in dto.ChiTiets)
                bienBan.ChiTiets.Add(new ChiTietBienBan { MucSo = ct.MucSo, PhanNhom = ct.PhanNhom, NoiDung = ct.NoiDung, Dat = ct.Dat, GhiChu = ct.GhiChu });

            foreach (var ck in dto.ChuKys)
                bienBan.ChuKys.Add(new ChuKy { ViTri = ck.ViTri, TenNguoiKy = ck.TenNguoiKy, ChucVuNguoiKy = ck.ChucVuNguoiKy, DuLieuChuKy = ck.DuLieuChuKy });

            if (dto.DinhLuongs != null)
            {
                foreach (var dl in dto.DinhLuongs)
                    bienBan.DinhLuongs.Add(new DinhLuongSuatAn
                    {
                        STT = dl.STT, LoaiSuatAn = dl.LoaiSuatAn, NoiDung = dl.NoiDung,
                        CheDoAn1Ten = dl.CheDoAn1Ten, CheDoAn1KhoiLuong = dl.CheDoAn1KhoiLuong,
                        CheDoAn1Dat = dl.CheDoAn1Dat, CheDoAn1KhongDat = dl.CheDoAn1KhongDat,
                        CheDoAn2Ten = dl.CheDoAn2Ten, CheDoAn2KhoiLuong = dl.CheDoAn2KhoiLuong,
                        CheDoAn2Dat = dl.CheDoAn2Dat, CheDoAn2KhongDat = dl.CheDoAn2KhongDat
                    });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật biên bản thành công" });
        }

        // ============================
        // PATCH: api/BienBan/5/gui
        // Employee gửi biên bản (ChuaGui → DaGui)
        // ============================
        [HttpPatch("{id}/gui")]
        public async Task<IActionResult> GuiBienBan(int id)
        {
            var bienBan = await _context.BienBans.FindAsync(id);
            if (bienBan == null) return NotFound();

            var userId = GetUserId();
            if (bienBan.NguoiTaoId != userId) return Forbid();

            if (bienBan.TrangThai != "ChuaGui" && bienBan.TrangThai != "TuChoi")
                return BadRequest(new { message = "Biên bản này không thể gửi lại" });

            bienBan.TrangThai = "DaGui";
            bienBan.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã gửi biên bản thành công" });
        }

        // ============================
        // PATCH: api/BienBan/5/duyet
        // Admin duyệt/từ chối biên bản
        // ============================
        [HttpPatch("{id}/duyet")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DuyetBienBan(int id, [FromBody] CapNhatTrangThaiDto dto)
        {
            var bienBan = await _context.BienBans.FindAsync(id);
            if (bienBan == null) return NotFound();

            var trangThaiHopLe = new[] { "DaDuyet", "TuChoi" };
            if (!trangThaiHopLe.Contains(dto.TrangThai))
                return BadRequest(new { message = "Trạng thái không hợp lệ. Chấp nhận: DaDuyet, TuChoi" });

            bienBan.TrangThai = dto.TrangThai;
            bienBan.NgayCapNhat = DateTime.UtcNow;

            // Nếu từ chối thì thêm ghi chú lý do
            if (!string.IsNullOrEmpty(dto.GhiChu))
                bienBan.YKienBPCB = (bienBan.YKienBPCB ?? "") + "\n[Admin] " + dto.GhiChu;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Biên bản đã được {(dto.TrangThai == "DaDuyet" ? "duyệt" : "từ chối")}" });
        }

        // ============================
        // DELETE: api/BienBan/5
        // Xóa biên bản (chỉ khi chưa gửi)
        // ============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bienBan = await _context.BienBans.FindAsync(id);
            if (bienBan == null) return NotFound();

            var userId = GetUserId();
            var userRole = GetUserRole();

            if (userRole != "Admin" && bienBan.NguoiTaoId != userId)
                return Forbid();

            if (userRole != "Admin" && bienBan.TrangThai != "ChuaGui")
                return BadRequest(new { message = "Không thể xóa biên bản đã gửi" });

            _context.BienBans.Remove(bienBan);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa biên bản" });
        }

        // ============================
        // Helper Methods
        // ============================
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "NguoiDung";
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
