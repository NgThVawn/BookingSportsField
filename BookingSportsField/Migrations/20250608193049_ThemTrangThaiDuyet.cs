using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSportsField.Migrations
{
    /// <inheritdoc />
    public partial class ThemTrangThaiDuyet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Facilities");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Facilities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Facilities");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Facilities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
