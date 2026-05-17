namespace KhaoSatBep.API.Models
{
    /// <summary>
    /// Bảng định lượng suất ăn - BB4 (Suất ăn đường miệng + ống thông)
    /// Mỗi hàng = 1 loại thức ăn, có 2 chế độ ăn
    /// </summary>
    public class DinhLuongSuatAn
    {
        public int Id { get; set; }
        public int STT { get; set; } // 1-9 (đường miệng) hoặc 1-6 (ống thông)
        public string LoaiSuatAn { get; set; } = string.Empty; // "DuongMieng" hoặc "OngThong"
        public string NoiDung { get; set; } = string.Empty; // VD: "Cơm", "Món mặn", "Món canh"...

        // Chế độ ăn 1
        public string? CheDoAn1Ten { get; set; } // Tên chế độ ăn 1
        public double? CheDoAn1KhoiLuong { get; set; } // Khối lượng (g)
        public bool? CheDoAn1Dat { get; set; } // Đ
        public bool? CheDoAn1KhongDat { get; set; } // KĐ

        // Chế độ ăn 2
        public string? CheDoAn2Ten { get; set; } // Tên chế độ ăn 2
        public double? CheDoAn2KhoiLuong { get; set; } // Khối lượng (g)
        public bool? CheDoAn2Dat { get; set; } // Đ
        public bool? CheDoAn2KhongDat { get; set; } // KĐ

        // Foreign Key
        public int BienBanId { get; set; }
        public BienBan BienBan { get; set; } = null!;
    }
}
