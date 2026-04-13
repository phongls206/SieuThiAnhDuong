using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SieuThiAnhDuong.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GioiTinh",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GioiTinh",
                table: "NhanViens");
        }
    }
}
