namespace KhaoSatBep.API.Models
{
    public class ChiTietKhaoSat
    {
        public int Id { get; set; }
        public string HangMuc { get; set; } = string.Empty; // Ve sinh, An toan thuc pham, Co so vat chat...
        public int Diem { get; set; } // 1-10
        public string? NhanXet { get; set; }

        // Foreign Key
        public int KhaoSatId { get; set; }

        // Navigation
        public KhaoSat KhaoSat { get; set; } = null!;
    }
}
