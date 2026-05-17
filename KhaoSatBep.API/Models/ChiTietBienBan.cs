namespace KhaoSatBep.API.Models
{
    /// <summary>
    /// Chi tiết từng mục kiểm tra trong biên bản
    /// Dùng chung cho BB1 (13 mục), BB2 (16 mục), BB3 (116 mục), BB4 yêu cầu riêng (7 mục)
    /// </summary>
    public class ChiTietBienBan
    {
        public int Id { get; set; }
        public int MucSo { get; set; } // Số thứ tự mục: 1, 2, 3...
        public string PhanNhom { get; set; } = string.Empty; // Section/Group name (VD: "Điều kiện con người", "Dụng cụ chế biến"...)
        public string NoiDung { get; set; } = string.Empty; // Nội dung kiểm tra
        public bool? Dat { get; set; } // true = Đạt, false = Không đạt, null = Chưa đánh giá
        public string? GhiChu { get; set; } // Ghi chú bổ sung

        // Foreign Key
        public int BienBanId { get; set; }
        public BienBan BienBan { get; set; } = null!;
    }
}
