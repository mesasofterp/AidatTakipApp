using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTCNOBoyKiloAdresToOgrenciler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adres",
                table: "Ogrenciler",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Boy",
                table: "Ogrenciler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Kilo",
                table: "Ogrenciler",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TCNO",
                table: "Ogrenciler",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adres",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "Boy",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "Kilo",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "TCNO",
                table: "Ogrenciler");
        }
    }
}
