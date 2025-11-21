using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOgrenciBasariMaclari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OgrenciBasariMaclari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasariId = table.Column<long>(type: "bigint", nullable: false),
                    RakipAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Skor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sonuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Lokasyon = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciBasariMaclari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciBasariMaclari_OgrenciBasarilari_BasariId",
                        column: x => x.BasariId,
                        principalTable: "OgrenciBasarilari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciBasariMaclari_BasariId",
                table: "OgrenciBasariMaclari",
                column: "BasariId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciBasariMaclari");
        }
    }
}
