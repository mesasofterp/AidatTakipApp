using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnOdemePeriyotlariIdToOdemePlanlariId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Önce foreign key constraint'i kaldır
            migrationBuilder.DropForeignKey(
                name: "FK_Ogrenciler_OdemePeriyotlari_OdemePeriyotlariId",
                table: "Ogrenciler");

            // Kolonu yeniden adlandır
            migrationBuilder.RenameColumn(
                name: "OdemePeriyotlariId",
                table: "Ogrenciler",
                newName: "OdemePlanlariId");

            // Yeni index oluştur (eğer yoksa)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ogrenciler_OdemePlanlariId' AND object_id = OBJECT_ID('Ogrenciler'))
                BEGIN
                    CREATE INDEX IX_Ogrenciler_OdemePlanlariId ON Ogrenciler(OdemePlanlariId);
                END
            ");

            // Yeni foreign key ekle
            migrationBuilder.AddForeignKey(
                name: "FK_Ogrenciler_OdemePlanlari_OdemePlanlariId",
                table: "Ogrenciler",
                column: "OdemePlanlariId",
                principalTable: "OdemePlanlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Foreign key'i kaldır
            migrationBuilder.DropForeignKey(
                name: "FK_Ogrenciler_OdemePlanlari_OdemePlanlariId",
                table: "Ogrenciler");

            // Index'i kaldır
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ogrenciler_OdemePlanlariId' AND object_id = OBJECT_ID('Ogrenciler'))
                BEGIN
                    DROP INDEX IX_Ogrenciler_OdemePlanlariId ON Ogrenciler;
                END
            ");

            // Kolonu eski ismine döndür
            migrationBuilder.RenameColumn(
                name: "OdemePlanlariId",
                table: "Ogrenciler",
                newName: "OdemePeriyotlariId");

            // Eski index'i oluştur
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ogrenciler_OdemePeriyotlariId' AND object_id = OBJECT_ID('Ogrenciler'))
                BEGIN
                    CREATE INDEX IX_Ogrenciler_OdemePeriyotlariId ON Ogrenciler(OdemePeriyotlariId);
                END
            ");

            // Eski foreign key'i ekle
            migrationBuilder.AddForeignKey(
                name: "FK_Ogrenciler_OdemePeriyotlari_OdemePeriyotlariId",
                table: "Ogrenciler",
                column: "OdemePeriyotlariId",
                principalTable: "OdemePeriyotlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
