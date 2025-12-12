using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSeansGunlerForMultipleGunSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "GunId",
                table: "Seanslar",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "SeansGunler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GunId = table.Column<long>(type: "bigint", nullable: false),
                    SeansId = table.Column<long>(type: "bigint", nullable: false),
                    SeanslarId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeansGunler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeansGunler_Gunler",
                        column: x => x.GunId,
                        principalTable: "Gunler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeansGunler_Seanslar",
                        column: x => x.SeansId,
                        principalTable: "Seanslar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeansGunler_Seanslar_SeanslarId",
                        column: x => x.SeanslarId,
                        principalTable: "Seanslar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeansGunler_GunId",
                table: "SeansGunler",
                column: "GunId");

            migrationBuilder.CreateIndex(
                name: "IX_SeansGunler_SeansId_GunId",
                table: "SeansGunler",
                columns: new[] { "SeansId", "GunId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeansGunler_SeanslarId",
                table: "SeansGunler",
                column: "SeanslarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeansGunler");

            migrationBuilder.AlterColumn<long>(
                name: "GunId",
                table: "Seanslar",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
