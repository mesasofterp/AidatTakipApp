using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSeanslarAndGunlerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SeansId",
                table: "Ogrenciler",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Gunler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Gun = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gunler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seanslar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeansAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SeansBaslangicSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    SeansBitisSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    SeansKapasitesi = table.Column<int>(type: "int", nullable: true),
                    SeansMevcudu = table.Column<int>(type: "int", nullable: true),
                    GunId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seanslar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seanslar_Gunler_GunId",
                        column: x => x.GunId,
                        principalTable: "Gunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_SeansId",
                table: "Ogrenciler",
                column: "SeansId");

            migrationBuilder.CreateIndex(
                name: "IX_Seanslar_GunId",
                table: "Seanslar",
                column: "GunId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ogrenciler_Seanslar_SeansId",
                table: "Ogrenciler",
                column: "SeansId",
                principalTable: "Seanslar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ogrenciler_Seanslar_SeansId",
                table: "Ogrenciler");

            migrationBuilder.DropTable(
                name: "Seanslar");

            migrationBuilder.DropTable(
                name: "Gunler");

            migrationBuilder.DropIndex(
                name: "IX_Ogrenciler_SeansId",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "SeansId",
                table: "Ogrenciler");
        }
    }
}
