using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class FilteredEmailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut Email unique index'ini kaldır
            migrationBuilder.DropIndex(
                name: "IX_Ogrenciler_Email",
                table: "Ogrenciler");

            // Email için filtered unique index oluştur (sadece IsDeleted = false olanlar için)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IX_Ogrenciler_Email 
                ON Ogrenciler(Email) 
                WHERE IsDeleted = 0;
            ");

            // TCNO için filtered unique index oluştur (sadece IsDeleted = false ve NULL olmayan için)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Ogrenciler_TCNO' AND object_id = OBJECT_ID('Ogrenciler'))
                BEGIN
                    CREATE UNIQUE INDEX IX_Ogrenciler_TCNO 
                    ON Ogrenciler(TCNO) 
                    WHERE IsDeleted = 0 AND TCNO IS NOT NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Filtered index'leri kaldır
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Ogrenciler_Email' AND object_id = OBJECT_ID('Ogrenciler'))
                DROP INDEX IX_Ogrenciler_Email ON Ogrenciler;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Ogrenciler_TCNO' AND object_id = OBJECT_ID('Ogrenciler'))
                DROP INDEX IX_Ogrenciler_TCNO ON Ogrenciler;
            ");

            // Eski unique index'i geri ekle (sadece Email için)
            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_Email",
                table: "Ogrenciler",
                column: "Email",
                unique: true);
        }
    }
}
