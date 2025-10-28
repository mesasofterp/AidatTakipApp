using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedCinsiyetlerData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Erkek ve Kadın cinsiyetlerini ekle
            migrationBuilder.InsertData(
                table: "Cinsiyetler",
                columns: new[] { "Cinsiyet" },
                values: new object[] { "Erkek" });

            migrationBuilder.InsertData(
                table: "Cinsiyetler",
                columns: new[] { "Cinsiyet" },
                values: new object[] { "Kadın" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Migration geri alındığında eklenen kayıtları sil
            migrationBuilder.DeleteData(
                table: "Cinsiyetler",
                keyColumn: "Cinsiyet",
                keyValue: "Erkek");

            migrationBuilder.DeleteData(
                table: "Cinsiyetler",
                keyColumn: "Cinsiyet",
                keyValue: "Kadın");
        }
    }
}
