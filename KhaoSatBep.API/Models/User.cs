namespace KhaoSatBep.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MatKhauHash { get; set; } = string.Empty;
        public string VaiTro { get; set; } = "NguoiDung"; // Admin, NguoiDung
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<KhaoSat> KhaoSats { get; set; } = new List<KhaoSat>();
        public ICollection<BienBan> BienBans { get; set; } = new List<BienBan>();
    }
}
