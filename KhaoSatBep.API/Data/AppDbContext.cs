using KhaoSatBep.API.Models;
using Microsoft.EntityFrameworkCore;

namespace KhaoSatBep.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ============================
        // Bảng cũ (giữ nguyên)
        // ============================
        public DbSet<User> Users { get; set; }
        public DbSet<Bep> Beps { get; set; }
        public DbSet<KhaoSat> KhaoSats { get; set; }
        public DbSet<ChiTietKhaoSat> ChiTietKhaoSats { get; set; }

        // ============================
        // Bảng mới - 4 Biên bản
        // ============================
        public DbSet<BienBan> BienBans { get; set; }
        public DbSet<ThanhPhanKiemTra> ThanhPhanKiemTras { get; set; }
        public DbSet<ChiTietBienBan> ChiTietBienBans { get; set; }
        public DbSet<DinhLuongSuatAn> DinhLuongSuatAns { get; set; }
        public DbSet<ChuKy> ChuKys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // User
            // ============================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.VaiTro).HasDefaultValue("NguoiDung");
            });

            // ============================
            // Bep (cũ)
            // ============================
            modelBuilder.Entity<Bep>(entity =>
            {
                entity.Property(b => b.TenBep).IsRequired().HasMaxLength(200);
                entity.Property(b => b.DiaChi).IsRequired().HasMaxLength(500);
            });

            // ============================
            // KhaoSat (cũ)
            // ============================
            modelBuilder.Entity<KhaoSat>(entity =>
            {
                entity.HasOne(k => k.Bep)
                    .WithMany(b => b.KhaoSats)
                    .HasForeignKey(k => k.BepId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(k => k.NguoiKhaoSat)
                    .WithMany(u => u.KhaoSats)
                    .HasForeignKey(k => k.NguoiKhaoSatId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // ChiTietKhaoSat (cũ)
            // ============================
            modelBuilder.Entity<ChiTietKhaoSat>(entity =>
            {
                entity.HasOne(c => c.KhaoSat)
                    .WithMany(k => k.ChiTiets)
                    .HasForeignKey(c => c.KhaoSatId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.Diem).IsRequired();
            });

            // ============================
            // BienBan (mới)
            // ============================
            modelBuilder.Entity<BienBan>(entity =>
            {
                entity.Property(b => b.SoBienBan).IsRequired().HasMaxLength(50);
                entity.Property(b => b.LoaiBienBan).IsRequired().HasMaxLength(20);
                entity.Property(b => b.TrangThai).IsRequired().HasMaxLength(20).HasDefaultValue("ChuaGui");
                entity.Property(b => b.GopYKhoaDinhDuong).HasMaxLength(2000);
                entity.Property(b => b.YKienBPCB).HasMaxLength(2000);
                entity.Property(b => b.BuaAnDuongMieng).HasMaxLength(100);
                entity.Property(b => b.ThucDonHangNgay).HasMaxLength(50);
                entity.Property(b => b.BuaAnOngThong).HasMaxLength(100);

                entity.HasIndex(b => b.LoaiBienBan);
                entity.HasIndex(b => b.NgayKiemTra);
                entity.HasIndex(b => b.TrangThai);

                entity.HasOne(b => b.NguoiTao)
                    .WithMany(u => u.BienBans)
                    .HasForeignKey(b => b.NguoiTaoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // ThanhPhanKiemTra
            // ============================
            modelBuilder.Entity<ThanhPhanKiemTra>(entity =>
            {
                entity.Property(t => t.HoTen).IsRequired().HasMaxLength(200);
                entity.Property(t => t.ChucVu).IsRequired().HasMaxLength(200);

                entity.HasOne(t => t.BienBan)
                    .WithMany(b => b.ThanhPhans)
                    .HasForeignKey(t => t.BienBanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // ChiTietBienBan
            // ============================
            modelBuilder.Entity<ChiTietBienBan>(entity =>
            {
                entity.Property(c => c.PhanNhom).IsRequired().HasMaxLength(200);
                entity.Property(c => c.NoiDung).IsRequired().HasMaxLength(1000);
                entity.Property(c => c.GhiChu).HasMaxLength(500);

                entity.HasIndex(c => c.MucSo);

                entity.HasOne(c => c.BienBan)
                    .WithMany(b => b.ChiTiets)
                    .HasForeignKey(c => c.BienBanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // DinhLuongSuatAn
            // ============================
            modelBuilder.Entity<DinhLuongSuatAn>(entity =>
            {
                entity.Property(d => d.LoaiSuatAn).IsRequired().HasMaxLength(20);
                entity.Property(d => d.NoiDung).IsRequired().HasMaxLength(200);
                entity.Property(d => d.CheDoAn1Ten).HasMaxLength(100);
                entity.Property(d => d.CheDoAn2Ten).HasMaxLength(100);

                entity.HasOne(d => d.BienBan)
                    .WithMany(b => b.DinhLuongs)
                    .HasForeignKey(d => d.BienBanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // ChuKy
            // ============================
            modelBuilder.Entity<ChuKy>(entity =>
            {
                entity.Property(c => c.ViTri).IsRequired().HasMaxLength(20);
                entity.Property(c => c.TenNguoiKy).IsRequired().HasMaxLength(200);
                entity.Property(c => c.ChucVuNguoiKy).IsRequired().HasMaxLength(200);
                // DuLieuChuKy là Base64 nên không giới hạn maxlength (nvarchar(max))

                entity.HasOne(c => c.BienBan)
                    .WithMany(b => b.ChuKys)
                    .HasForeignKey(c => c.BienBanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // Seed data - tài khoản mặc định
            // ============================
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    HoTen = "Quản Trị Viên",
                    Email = "admin@khaosatbep.vn",
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    VaiTro = "Admin",
                    NgayTao = new DateTime(2025, 1, 1)
                },
                new User
                {
                    Id = 2,
                    HoTen = "Nhân Viên Khảo Sát",
                    Email = "nhanvien@khaosatbep.vn",
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Nv@123"),
                    VaiTro = "NguoiDung",
                    NgayTao = new DateTime(2025, 1, 1)
                }
            );
        }
    }
}
