using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOkulGirisCikisSaatiToOgrenciDetay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "OkulCikisSaati",
                table: "OgrenciDetay",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OkulGirisSaati",
                table: "OgrenciDetay",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OkulCikisSaati",
                table: "OgrenciDetay");

            migrationBuilder.DropColumn(
                name: "OkulGirisSaati",
                table: "OgrenciDetay");
        }
    }
}
