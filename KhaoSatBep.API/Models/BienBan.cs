namespace KhaoSatBep.API.Models
{
    /// <summary>
    /// Bảng chính - Header cho tất cả biên bản
    /// LoaiBienBan: "CSHT" (BB1), "HoSo" (BB2), "VeSinh" (BB3), "SuatAn" (BB4)
    /// </summary>
    public class BienBan
    {
        public int Id { get; set; }
        public string SoBienBan { get; set; } = string.Empty; // Số BB-...
        public string LoaiBienBan { get; set; } = string.Empty; // CSHT, HoSo, VeSinh, SuatAn
        public DateTime NgayKiemTra { get; set; }
        public string TrangThai { get; set; } = "ChuaGui"; // ChuaGui, DaGui, DaDuyet, TuChoi
        public string? GopYKhoaDinhDuong { get; set; } // Góp ý, nhắc nhở của Khoa Dinh dưỡng
        public string? YKienBPCB { get; set; } // Ý kiến của BPCB & CCSA
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public DateTime? NgayCapNhat { get; set; }

        // Foreign Key - Người tạo biên bản
        public int NguoiTaoId { get; set; }
        public User NguoiTao { get; set; } = null!;

        // Navigation
        public ICollection<ThanhPhanKiemTra> ThanhPhans { get; set; } = new List<ThanhPhanKiemTra>();
        public ICollection<ChiTietBienBan> ChiTiets { get; set; } = new List<ChiTietBienBan>();
        public ICollection<DinhLuongSuatAn> DinhLuongs { get; set; } = new List<DinhLuongSuatAn>();
        public ICollection<ChuKy> ChuKys { get; set; } = new List<ChuKy>();

        // BB4 fields - Suất ăn đường miệng
        public string? BuaAnDuongMieng { get; set; } // "Sang,Trua,Xe,Chieu,Toi" (comma-separated)
        public string? ThucDonHangNgay { get; set; } // "ThayDoi" hoặc "KhongThayDoi"

        // BB4 fields - Suất ăn qua ống thông
        public string? BuaAnOngThong { get; set; } // "Sang,Trua,Xe,Chieu" (comma-separated)
    }
}
