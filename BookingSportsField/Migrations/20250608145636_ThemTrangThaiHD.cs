using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSportsField.Migrations
{
    /// <inheritdoc />
    public partial class ThemTrangThaiHD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Fields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Facilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Facilities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Facilities");
        }
    }
}
