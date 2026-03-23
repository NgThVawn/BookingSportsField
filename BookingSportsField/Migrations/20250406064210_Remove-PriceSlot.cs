using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSportsField.Migrations
{
    /// <inheritdoc />
    public partial class RemovePriceSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldPricingSlots");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "Fields",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "Fields");

            migrationBuilder.CreateTable(
                name: "FieldPricingSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PricePerHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPricingSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldPricingSlots_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldPricingSlots_FieldId",
                table: "FieldPricingSlots",
                column: "FieldId");
        }
    }
}
