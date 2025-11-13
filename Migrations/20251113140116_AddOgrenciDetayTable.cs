using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOgrenciDetayTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OgrenciDetay",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    VeliAdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VeliTelefonNumarasi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OkulAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OkulAdresi = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Sinif = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OkulHocasiAdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OkulHocasiTelefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciDetay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciDetay_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDetay_OgrenciId",
                table: "OgrenciDetay",
                column: "OgrenciId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciDetay");
        }
    }
}
