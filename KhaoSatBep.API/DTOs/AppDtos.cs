namespace KhaoSatBep.API.DTOs
{
    // Auth DTOs
    public class DangNhapDto
    {
        public string Email { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
    }

    public class DangKyDto
    {
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
    }

    // Bep DTOs
    public class BepDto
    {
        public int Id { get; set; }
        public string TenBep { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string LoaiBep { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public DateTime NgayTao { get; set; }
        public int SoLuongKhaoSat { get; set; }
    }

    public class TaoBepDto
    {
        public string TenBep { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string LoaiBep { get; set; } = string.Empty;
        public string? MoTa { get; set; }
    }

    // KhaoSat DTOs
    public class KhaoSatDto
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public DateTime NgayKhaoSat { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; }
        public string TenBep { get; set; } = string.Empty;
        public string NguoiKhaoSat { get; set; } = string.Empty;
        public double DiemTrungBinh { get; set; }
    }

    public class TaoKhaoSatDto
    {
        public string TieuDe { get; set; } = string.Empty;
        public DateTime NgayKhaoSat { get; set; }
        public string? GhiChu { get; set; }
        public int BepId { get; set; }
        public List<TaoChiTietDto> ChiTiets { get; set; } = new();
    }

    public class CapNhatKhaoSatDto
    {
        public string TieuDe { get; set; } = string.Empty;
        public DateTime NgayKhaoSat { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    // ChiTiet DTOs
    public class ChiTietDto
    {
        public int Id { get; set; }
        public string HangMuc { get; set; } = string.Empty;
        public int Diem { get; set; }
        public string? NhanXet { get; set; }
    }

    public class TaoChiTietDto
    {
        public string HangMuc { get; set; } = string.Empty;
        public int Diem { get; set; }
        public string? NhanXet { get; set; }
    }
}
