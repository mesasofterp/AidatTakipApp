using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class eaden222 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cinsiyetler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cinsiyet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cinsiyetler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OdemePlanlari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KursProgrami = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Taksit = table.Column<int>(type: "int", nullable: false),
                    Vade = table.Column<int>(type: "int", nullable: true),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdemePlanlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZamanlayiciAyarlar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Isim = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Saat = table.Column<int>(type: "int", nullable: false),
                    Dakika = table.Column<int>(type: "int", nullable: false),
                    CronIfadesi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MesajSablonu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZamanlayiciAyarlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ogrenciler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OgrenciSoyadi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdemePlanlariId = table.Column<long>(type: "bigint", nullable: false),
                    CinsiyetId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ogrenciler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_Cinsiyetler_CinsiyetId",
                        column: x => x.CinsiyetId,
                        principalTable: "Cinsiyetler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_OdemePlanlari_OdemePlanlariId",
                        column: x => x.OdemePlanlariId,
                        principalTable: "OdemePlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciOdemeTakvimi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SonOdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OdenenTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BorcTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciOdemeTakvimi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciOdemeTakvimi_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_CinsiyetId",
                table: "Ogrenciler",
                column: "CinsiyetId");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_Email",
                table: "Ogrenciler",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_OdemePlanlariId",
                table: "Ogrenciler",
                column: "OdemePlanlariId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciOdemeTakvimi_OgrenciId",
                table: "OgrenciOdemeTakvimi",
                column: "OgrenciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciOdemeTakvimi");

            migrationBuilder.DropTable(
                name: "ZamanlayiciAyarlar");

            migrationBuilder.DropTable(
                name: "Ogrenciler");

            migrationBuilder.DropTable(
                name: "Cinsiyetler");

            migrationBuilder.DropTable(
                name: "OdemePlanlari");
        }
    }
}
