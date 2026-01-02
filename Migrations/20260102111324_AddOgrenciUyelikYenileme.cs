using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOgrenciUyelikYenileme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OgrenciUyelikYenileme",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    EskiOdemePlaniId = table.Column<long>(type: "bigint", nullable: false),
                    YeniOdemePlaniId = table.Column<long>(type: "bigint", nullable: false),
                    YenilemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YenilemeBaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EskiDonemToplamTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YeniDonemToplamTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EskiDonemKalanBorc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IndirimTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IndirimAciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OtomatikYenileme = table.Column<bool>(type: "bit", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    YenileyenKullanici = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciUyelikYenileme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciUyelikYenileme_OdemePlanlari_EskiOdemePlaniId",
                        column: x => x.EskiOdemePlaniId,
                        principalTable: "OdemePlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OgrenciUyelikYenileme_OdemePlanlari_YeniOdemePlaniId",
                        column: x => x.YeniOdemePlaniId,
                        principalTable: "OdemePlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OgrenciUyelikYenileme_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciUyelikYenileme_EskiOdemePlaniId",
                table: "OgrenciUyelikYenileme",
                column: "EskiOdemePlaniId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciUyelikYenileme_OgrenciId",
                table: "OgrenciUyelikYenileme",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciUyelikYenileme_YeniOdemePlaniId",
                table: "OgrenciUyelikYenileme",
                column: "YeniOdemePlaniId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciUyelikYenileme");
        }
    }
}
