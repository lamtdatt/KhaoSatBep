namespace KhaoSatBep.API.Models
{
    /// <summary>
    /// Thành phần tham gia kiểm tra (mỗi BB có 3-4 người)
    /// </summary>
    public class ThanhPhanKiemTra
    {
        public int Id { get; set; }
        public int STT { get; set; } // Thứ tự: 1, 2, 3, 4
        public string HoTen { get; set; } = string.Empty;
        public string ChucVu { get; set; } = string.Empty;

        // Foreign Key
        public int BienBanId { get; set; }
        public BienBan BienBan { get; set; } = null!;
    }
}
