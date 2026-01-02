using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOgrenciUyelikDondurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OgrenciUyelikDondurma",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sebep = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OdemeTarihleriAyarlandi = table.Column<bool>(type: "bit", nullable: false),
                    KaydirilanGunSayisi = table.Column<int>(type: "int", nullable: false),
                    IptalTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IptalEdenKullanici = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IptalNedeni = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciUyelikDondurma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciUyelikDondurma_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciUyelikDondurma_OgrenciId",
                table: "OgrenciUyelikDondurma",
                column: "OgrenciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciUyelikDondurma");
        }
    }
}
