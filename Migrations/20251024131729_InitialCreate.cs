using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        // Tablolar zaten mevcut, bu migration sadece geçmiş için ekleniyor
   // Hiçbir şey yapılmasına gerek yok
        }

   /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
      // Rollback işlemi yok
        }
    }
}
