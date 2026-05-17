namespace KhaoSatBep.API.Models
{
    public class KhaoSat
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public DateTime NgayKhaoSat { get; set; }
        public string TrangThai { get; set; } = "ChoDuyet"; // ChoDuyet, DaDuyet, TuChoi
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Foreign Keys
        public int BepId { get; set; }
        public int NguoiKhaoSatId { get; set; }

        // Navigation
        public Bep Bep { get; set; } = null!;
        public User NguoiKhaoSat { get; set; } = null!;
        public ICollection<ChiTietKhaoSat> ChiTiets { get; set; } = new List<ChiTietKhaoSat>();
    }
}
