namespace KhaoSatBep.API.Models
{
    /// <summary>
    /// Chữ ký điện tử bằng Canvas (lưu dạng Base64 image)
    /// Mỗi biên bản có 2 chữ ký (bên trái + bên phải)
    /// </summary>
    public class ChuKy
    {
        public int Id { get; set; }
        public string ViTri { get; set; } = string.Empty; // "BenTrai" (Khoa DD / Bộ phận phụ trách), "BenPhai" (BPCB & CCSA)
        public string TenNguoiKy { get; set; } = string.Empty;
        public string ChucVuNguoiKy { get; set; } = string.Empty;
        public string DuLieuChuKy { get; set; } = string.Empty; // Base64 encoded canvas image (data:image/png;base64,...)
        public DateTime NgayKy { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int BienBanId { get; set; }
        public BienBan BienBan { get; set; } = null!;
    }
}
