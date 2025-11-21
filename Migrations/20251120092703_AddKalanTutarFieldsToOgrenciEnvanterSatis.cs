using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddKalanTutarFieldsToOgrenciEnvanterSatis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "KalanTutar",
                table: "OgrenciEnvanterSatis",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "KalanTutarTahsilTarihi",
                table: "OgrenciEnvanterSatis",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KalanTutar",
                table: "OgrenciEnvanterSatis");

            migrationBuilder.DropColumn(
                name: "KalanTutarTahsilTarihi",
                table: "OgrenciEnvanterSatis");
        }
    }
}
