namespace KhaoSatBep.API.DTOs
{
    // =============================================
    // DTOs dùng chung cho tất cả Biên bản
    // =============================================

    /// <summary>
    /// Thành phần tham gia kiểm tra
    /// </summary>
    public class ThanhPhanDto
    {
        public int STT { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string ChucVu { get; set; } = string.Empty;
    }

    /// <summary>
    /// Chi tiết mục kiểm tra (Đạt/KĐ/Ghi chú)
    /// </summary>
    public class ChiTietBienBanDto
    {
        public int Id { get; set; }
        public int MucSo { get; set; }
        public string PhanNhom { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public bool? Dat { get; set; }
        public string? GhiChu { get; set; }
    }

    /// <summary>
    /// Tạo mới chi tiết mục kiểm tra
    /// </summary>
    public class TaoChiTietBienBanDto
    {
        public int MucSo { get; set; }
        public string PhanNhom { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public bool? Dat { get; set; }
        public string? GhiChu { get; set; }
    }

    /// <summary>
    /// Chữ ký điện tử (canvas base64)
    /// </summary>
    public class ChuKyDto
    {
        public string ViTri { get; set; } = string.Empty; // "BenTrai", "BenPhai"
        public string TenNguoiKy { get; set; } = string.Empty;
        public string ChucVuNguoiKy { get; set; } = string.Empty;
        public string DuLieuChuKy { get; set; } = string.Empty; // Base64
    }

    /// <summary>
    /// Định lượng suất ăn (BB4)
    /// </summary>
    public class DinhLuongDto
    {
        public int STT { get; set; }
        public string LoaiSuatAn { get; set; } = string.Empty; // "DuongMieng" / "OngThong"
        public string NoiDung { get; set; } = string.Empty;
        public string? CheDoAn1Ten { get; set; }
        public double? CheDoAn1KhoiLuong { get; set; }
        public bool? CheDoAn1Dat { get; set; }
        public bool? CheDoAn1KhongDat { get; set; }
        public string? CheDoAn2Ten { get; set; }
        public double? CheDoAn2KhoiLuong { get; set; }
        public bool? CheDoAn2Dat { get; set; }
        public bool? CheDoAn2KhongDat { get; set; }
    }

    // =============================================
    // DTO TẠO BIÊN BẢN (dùng cho tất cả 4 loại)
    // =============================================

    /// <summary>
    /// DTO tạo biên bản mới — dùng chung cho BB1, BB2, BB3, BB4
    /// Frontend gửi lên tất cả data trong 1 request
    /// </summary>
    public class TaoBienBanDto
    {
        public string SoBienBan { get; set; } = string.Empty;
        public string LoaiBienBan { get; set; } = string.Empty; // CSHT, HoSo, VeSinh, SuatAn
        public DateTime NgayKiemTra { get; set; }
        public string? GopYKhoaDinhDuong { get; set; }
        public string? YKienBPCB { get; set; }

        // Thành phần
        public List<ThanhPhanDto> ThanhPhans { get; set; } = new();

        // Chi tiết kiểm tra
        public List<TaoChiTietBienBanDto> ChiTiets { get; set; } = new();

        // Chữ ký
        public List<ChuKyDto> ChuKys { get; set; } = new();

        // BB4 specific
        public string? BuaAnDuongMieng { get; set; }
        public string? ThucDonHangNgay { get; set; }
        public string? BuaAnOngThong { get; set; }
        public List<DinhLuongDto>? DinhLuongs { get; set; }
    }

    // =============================================
    // DTO RESPONSE BIÊN BẢN
    // =============================================

    /// <summary>
    /// DTO trả về danh sách biên bản (summary)
    /// </summary>
    public class BienBanSummaryDto
    {
        public int Id { get; set; }
        public string SoBienBan { get; set; } = string.Empty;
        public string LoaiBienBan { get; set; } = string.Empty;
        public DateTime NgayKiemTra { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string NguoiTao { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public int SoMucDat { get; set; }
        public int SoMucKhongDat { get; set; }
        public int TongSoMuc { get; set; }
    }

    /// <summary>
    /// DTO trả về chi tiết biên bản đầy đủ
    /// </summary>
    public class BienBanChiTietDto
    {
        public int Id { get; set; }
        public string SoBienBan { get; set; } = string.Empty;
        public string LoaiBienBan { get; set; } = string.Empty;
        public DateTime NgayKiemTra { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GopYKhoaDinhDuong { get; set; }
        public string? YKienBPCB { get; set; }
        public string NguoiTao { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }

        // BB4 specific
        public string? BuaAnDuongMieng { get; set; }
        public string? ThucDonHangNgay { get; set; }
        public string? BuaAnOngThong { get; set; }

        // Sub-collections
        public List<ThanhPhanDto> ThanhPhans { get; set; } = new();
        public List<ChiTietBienBanDto> ChiTiets { get; set; } = new();
        public List<ChuKyDto> ChuKys { get; set; } = new();
        public List<DinhLuongDto>? DinhLuongs { get; set; }
    }

    /// <summary>
    /// DTO cập nhật trạng thái biên bản (Admin duyệt/từ chối)
    /// </summary>
    public class CapNhatTrangThaiDto
    {
        public string TrangThai { get; set; } = string.Empty; // DaGui, DaDuyet, TuChoi
        public string? GhiChu { get; set; }
    }

    // =============================================
    // DTO THỐNG KÊ DASHBOARD
    // =============================================

    /// <summary>
    /// Thống kê tổng quan cho Dashboard
    /// </summary>
    public class ThongKeTongQuanDto
    {
        public int TongBienBan { get; set; }
        public int BienBanDaDuyet { get; set; }
        public int BienBanChoGui { get; set; }
        public int BienBanTuChoi { get; set; }
        public double TyLeDat { get; set; } // % mục đạt trên tổng
    }

    /// <summary>
    /// Thống kê theo thời gian (ngày/tuần/tháng)
    /// </summary>
    public class ThongKeTheoThoiGianDto
    {
        public string NhanThoiGian { get; set; } = string.Empty; // "12/05/2026", "Tuần 20", "Tháng 5"...
        public int SoBienBan { get; set; }
        public int SoMucDat { get; set; }
        public int SoMucKhongDat { get; set; }
    }

    /// <summary>
    /// Thống kê theo loại biên bản
    /// </summary>
    public class ThongKeTheoLoaiDto
    {
        public string LoaiBienBan { get; set; } = string.Empty;
        public string TenLoai { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public double TyLeDat { get; set; }
    }
}
