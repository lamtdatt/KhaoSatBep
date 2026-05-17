using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhaoSatBep.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "MatKhauHash",
                value: "$2a$11$f8q0uP5zlsfb.oonTFav6uf84rXnd18Vi.dF3ucy26.jfLupFY6xi");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "HoTen", "MatKhauHash", "NgayTao", "VaiTro" },
                values: new object[] { 2, "nhanvien@khaosatbep.vn", "Nhân Viên Khảo Sát", "$2a$11$k3UzoezRv4NLKcQYFQOFBOgLn6/lRTtOl02j9zKTU65Q9VysxSMF2", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NguoiDung" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "MatKhauHash",
                value: "$2a$11$PdFWfiloDUL0W70wTkq/lulgqO.UlBFQciWyGi0FAdqITjGhrq95K");
        }
    }
}
