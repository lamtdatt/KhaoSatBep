namespace KhaoSatBep.API.Models
{
    public class Bep
    {
        public int Id { get; set; }
        public string TenBep { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string LoaiBep { get; set; } = string.Empty; // Gia dinh, Nha hang, Truong hoc...
        public string? MoTa { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<KhaoSat> KhaoSats { get; set; } = new List<KhaoSat>();
    }
}
