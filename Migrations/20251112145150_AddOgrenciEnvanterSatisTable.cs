using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOgrenciEnvanterSatisTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OgrenciEnvanterSatis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<long>(type: "bigint", nullable: false),
                    SatisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OdenenTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EnvanterId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciEnvanterSatis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciEnvanterSatis_Envanterler_EnvanterId",
                        column: x => x.EnvanterId,
                        principalTable: "Envanterler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OgrenciEnvanterSatis_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciEnvanterSatis_EnvanterId",
                table: "OgrenciEnvanterSatis",
                column: "EnvanterId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciEnvanterSatis_OgrenciId",
                table: "OgrenciEnvanterSatis",
                column: "OgrenciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OgrenciEnvanterSatis");
        }
    }
}
