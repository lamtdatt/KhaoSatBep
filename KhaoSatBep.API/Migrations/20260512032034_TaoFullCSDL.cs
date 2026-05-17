using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhaoSatBep.API.Migrations
{
    /// <inheritdoc />
    public partial class TaoFullCSDL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenBep = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LoaiBep = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "NguoiDung"),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BienBans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoBienBan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoaiBienBan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayKiemTra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ChuaGui"),
                    GopYKhoaDinhDuong = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    YKienBPCB = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    BuaAnDuongMieng = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThucDonHangNgay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BuaAnOngThong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BienBans_Users_NguoiTaoId",
                        column: x => x.NguoiTaoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KhaoSats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayKhaoSat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BepId = table.Column<int>(type: "int", nullable: false),
                    NguoiKhaoSatId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhaoSats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhaoSats_Beps_BepId",
                        column: x => x.BepId,
                        principalTable: "Beps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KhaoSats_Users_NguoiKhaoSatId",
                        column: x => x.NguoiKhaoSatId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietBienBans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MucSo = table.Column<int>(type: "int", nullable: false),
                    PhanNhom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Dat = table.Column<bool>(type: "bit", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BienBanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietBienBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietBienBans_BienBans_BienBanId",
                        column: x => x.BienBanId,
                        principalTable: "BienBans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuKys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViTri = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenNguoiKy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChucVuNguoiKy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DuLieuChuKy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BienBanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuKys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChuKys_BienBans_BienBanId",
                        column: x => x.BienBanId,
                        principalTable: "BienBans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DinhLuongSuatAns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    STT = table.Column<int>(type: "int", nullable: false),
                    LoaiSuatAn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CheDoAn1Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CheDoAn1KhoiLuong = table.Column<double>(type: "float", nullable: true),
                    CheDoAn1Dat = table.Column<bool>(type: "bit", nullable: true),
                    CheDoAn1KhongDat = table.Column<bool>(type: "bit", nullable: true),
                    CheDoAn2Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CheDoAn2KhoiLuong = table.Column<double>(type: "float", nullable: true),
                    CheDoAn2Dat = table.Column<bool>(type: "bit", nullable: true),
                    CheDoAn2KhongDat = table.Column<bool>(type: "bit", nullable: true),
                    BienBanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DinhLuongSuatAns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DinhLuongSuatAns_BienBans_BienBanId",
                        column: x => x.BienBanId,
                        principalTable: "BienBans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThanhPhanKiemTras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    STT = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChucVu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BienBanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhPhanKiemTras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhPhanKiemTras_BienBans_BienBanId",
                        column: x => x.BienBanId,
                        principalTable: "BienBans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietKhaoSats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HangMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Diem = table.Column<int>(type: "int", nullable: false),
                    NhanXet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhaoSatId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietKhaoSats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietKhaoSats_KhaoSats_KhaoSatId",
                        column: x => x.KhaoSatId,
                        principalTable: "KhaoSats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "HoTen", "MatKhauHash", "NgayTao", "VaiTro" },
                values: new object[] { 1, "admin@khaosatbep.vn", "Quản Trị Viên", "$2a$11$PdFWfiloDUL0W70wTkq/lulgqO.UlBFQciWyGi0FAdqITjGhrq95K", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_BienBans_LoaiBienBan",
                table: "BienBans",
                column: "LoaiBienBan");

            migrationBuilder.CreateIndex(
                name: "IX_BienBans_NgayKiemTra",
                table: "BienBans",
                column: "NgayKiemTra");

            migrationBuilder.CreateIndex(
                name: "IX_BienBans_NguoiTaoId",
                table: "BienBans",
                column: "NguoiTaoId");

            migrationBuilder.CreateIndex(
                name: "IX_BienBans_TrangThai",
                table: "BienBans",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietBienBans_BienBanId",
                table: "ChiTietBienBans",
                column: "BienBanId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietBienBans_MucSo",
                table: "ChiTietBienBans",
                column: "MucSo");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKhaoSats_KhaoSatId",
                table: "ChiTietKhaoSats",
                column: "KhaoSatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuKys_BienBanId",
                table: "ChuKys",
                column: "BienBanId");

            migrationBuilder.CreateIndex(
                name: "IX_DinhLuongSuatAns_BienBanId",
                table: "DinhLuongSuatAns",
                column: "BienBanId");

            migrationBuilder.CreateIndex(
                name: "IX_KhaoSats_BepId",
                table: "KhaoSats",
                column: "BepId");

            migrationBuilder.CreateIndex(
                name: "IX_KhaoSats_NguoiKhaoSatId",
                table: "KhaoSats",
                column: "NguoiKhaoSatId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhPhanKiemTras_BienBanId",
                table: "ThanhPhanKiemTras",
                column: "BienBanId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietBienBans");

            migrationBuilder.DropTable(
                name: "ChiTietKhaoSats");

            migrationBuilder.DropTable(
                name: "ChuKys");

            migrationBuilder.DropTable(
                name: "DinhLuongSuatAns");

            migrationBuilder.DropTable(
                name: "ThanhPhanKiemTras");

            migrationBuilder.DropTable(
                name: "KhaoSats");

            migrationBuilder.DropTable(
                name: "BienBans");

            migrationBuilder.DropTable(
                name: "Beps");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
