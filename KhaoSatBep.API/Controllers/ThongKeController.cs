using KhaoSatBep.API.Data;
using KhaoSatBep.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace KhaoSatBep.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ThongKeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ThongKeController(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // GET: api/ThongKe/tong-quan
        // Thống kê tổng quan
        // ============================
        [HttpGet("tong-quan")]
        public async Task<ActionResult<ThongKeTongQuanDto>> GetTongQuan()
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            var query = _context.BienBans.AsQueryable();
            if (userRole != "Admin")
                query = query.Where(b => b.NguoiTaoId == userId);

            var tongBienBan = await query.CountAsync();
            var daDuyet = await query.CountAsync(b => b.TrangThai == "DaDuyet");
            var choGui = await query.CountAsync(b => b.TrangThai == "ChuaGui" || b.TrangThai == "DaGui");
            var tuChoi = await query.CountAsync(b => b.TrangThai == "TuChoi");

            // Tính tỷ lệ đạt
            var chiTietQuery = _context.ChiTietBienBans.AsQueryable();
            if (userRole != "Admin")
                chiTietQuery = chiTietQuery.Where(c => c.BienBan.NguoiTaoId == userId);

            var tongMuc = await chiTietQuery.CountAsync(c => c.Dat.HasValue);
            var mucDat = await chiTietQuery.CountAsync(c => c.Dat == true);
            var tyLeDat = tongMuc > 0 ? Math.Round((double)mucDat / tongMuc * 100, 1) : 0;

            return Ok(new ThongKeTongQuanDto
            {
                TongBienBan = tongBienBan,
                BienBanDaDuyet = daDuyet,
                BienBanChoGui = choGui,
                BienBanTuChoi = tuChoi,
                TyLeDat = tyLeDat
            });
        }

        // ============================
        // GET: api/ThongKe/theo-ngay?tuNgay=...&denNgay=...
        // Thống kê theo ngày
        // ============================
        [HttpGet("theo-ngay")]
        public async Task<ActionResult<List<ThongKeTheoThoiGianDto>>> GetTheoNgay(
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            var from = tuNgay ?? DateTime.Now.AddDays(-30);
            var to = denNgay ?? DateTime.Now;

            var query = _context.BienBans
                .Include(b => b.ChiTiets)
                .Where(b => b.NgayKiemTra >= from && b.NgayKiemTra <= to);

            if (userRole != "Admin")
                query = query.Where(b => b.NguoiTaoId == userId);

            var data = await query.ToListAsync();

            var result = data
                .GroupBy(b => b.NgayKiemTra.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ThongKeTheoThoiGianDto
                {
                    NhanThoiGian = g.Key.ToString("dd/MM/yyyy"),
                    SoBienBan = g.Count(),
                    SoMucDat = g.SelectMany(b => b.ChiTiets).Count(c => c.Dat == true),
                    SoMucKhongDat = g.SelectMany(b => b.ChiTiets).Count(c => c.Dat == false)
                })
                .ToList();

            return Ok(result);
        }

        // ============================
        // GET: api/ThongKe/theo-tuan?tuNgay=...&denNgay=...
        // Thống kê theo tuần
        // ============================
        [HttpGet("theo-tuan")]
        public async Task<ActionResult<List<ThongKeTheoThoiGianDto>>> GetTheoTuan(
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            var from = tuNgay ?? DateTime.Now.AddMonths(-3);
            var to = denNgay ?? DateTime.Now;

            var query = _context.BienBans
                .Include(b => b.ChiTiets)
                .Where(b => b.NgayKiemTra >= from && b.NgayKiemTra <= to);

            if (userRole != "Admin")
                query = query.Where(b => b.NguoiTaoId == userId);

            var data = await query.ToListAsync();

            var calendar = CultureInfo.CurrentCulture.Calendar;
            var result = data
                .GroupBy(b =>
                {
                    var weekNum = calendar.GetWeekOfYear(b.NgayKiemTra, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    return new { Year = b.NgayKiemTra.Year, Week = weekNum };
                })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week)
                .Select(g => new ThongKeTheoThoiGianDto
                {
                    NhanThoiGian = $"Tuần {g.Key.Week}/{g.Key.Year}",
                    SoBienBan = g.Count(),
                    SoMucDat = g.SelectMany(b => b.ChiTiets).Count(c => c.Dat == true),
                    SoMucKhongDat = g.SelectMany(b => b.ChiTiets).Count(c => c.Dat == false)
                })
                .ToList();

            return Ok(result);
        }

        // ============================
        // GET: api/ThongKe/theo-thang?nam=2026
        // Thống kê theo tháng
        // ============================
        [HttpGet("theo-thang")]
        public async Task<ActionResult<List<ThongKeTheoThoiGianDto>>> GetTheoThang([FromQuery] int? nam)
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            var year = nam ?? DateTime.Now.Year;

            var query = _context.BienBans
                .Include(b => b.ChiTiets)
                .Where(b => b.NgayKiemTra.Year == year);

            if (userRole != "Admin")
                query = query.Where(b => b.NguoiTaoId == userId);

            var data = await query.ToListAsync();

            var result = data
                .GroupBy(b => b.NgayKiemTra.Month)
                .OrderBy(g => g.Key)
                .Select(g => new ThongKeTheoThoiGianDto
                {
                    NhanThoiGian = $"Tháng {g.Key}",
                    SoBienBan = g.Count(),
                    SoMucDat = g.SelectMany(b => b.ChiTiets).Count(c => c.Dat == true),
                    SoMucKhongDat = g.SelectMany(b => b.ChiTiets).Count(c => c.Dat == false)
                })
                .ToList();

            return Ok(result);
        }

        // ============================
        // GET: api/ThongKe/theo-loai
        // Thống kê theo loại biên bản
        // ============================
        [HttpGet("theo-loai")]
        public async Task<ActionResult<List<ThongKeTheoLoaiDto>>> GetTheoLoai()
        {
            var userId = GetUserId();
            var userRole = GetUserRole();

            var query = _context.BienBans
                .Include(b => b.ChiTiets)
                .AsQueryable();

            if (userRole != "Admin")
                query = query.Where(b => b.NguoiTaoId == userId);

            var data = await query.ToListAsync();

            var tenLoai = new Dictionary<string, string>
            {
                { "CSHT", "Cơ sở hạ tầng" },
                { "HoSo", "Hồ sơ sổ sách" },
                { "VeSinh", "Vệ sinh ATTP" },
                { "SuatAn", "Suất ăn người bệnh" }
            };

            var result = data
                .GroupBy(b => b.LoaiBienBan)
                .Select(g =>
                {
                    var chiTiets = g.SelectMany(b => b.ChiTiets).Where(c => c.Dat.HasValue).ToList();
                    var mucDat = chiTiets.Count(c => c.Dat == true);
                    var tong = chiTiets.Count;

                    return new ThongKeTheoLoaiDto
                    {
                        LoaiBienBan = g.Key,
                        TenLoai = tenLoai.GetValueOrDefault(g.Key, g.Key),
                        SoLuong = g.Count(),
                        TyLeDat = tong > 0 ? Math.Round((double)mucDat / tong * 100, 1) : 0
                    };
                })
                .ToList();

            return Ok(result);
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
    }
}
