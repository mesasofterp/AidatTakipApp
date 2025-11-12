using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnvanterlerFiyatAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BirimFiyat",
                table: "Envanterler",
                newName: "SatisFiyat");

            migrationBuilder.AddColumn<decimal>(
                name: "AlisFiyat",
                table: "Envanterler",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlisFiyat",
                table: "Envanterler");

            migrationBuilder.RenameColumn(
                name: "SatisFiyat",
                table: "Envanterler",
                newName: "BirimFiyat");
        }
    }
}
