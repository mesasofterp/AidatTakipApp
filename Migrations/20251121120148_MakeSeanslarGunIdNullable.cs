using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeSeanslarGunIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint if exists
            migrationBuilder.Sql(@"
                IF EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
     WHERE name = 'FK_Seanslar_Gunler_GunId' 
        AND parent_object_id = OBJECT_ID('Seanslar')
         )
       BEGIN
  ALTER TABLE Seanslar DROP CONSTRAINT FK_Seanslar_Gunler_GunId
        END
  ");

            // Make GunId nullable
            migrationBuilder.Sql(@"
   IF EXISTS (
     SELECT 1 
      FROM INFORMATION_SCHEMA.COLUMNS 
  WHERE TABLE_NAME = 'Seanslar' 
     AND COLUMN_NAME = 'GunId'
    AND IS_NULLABLE = 'NO'
     )
 BEGIN
    ALTER TABLE Seanslar ALTER COLUMN GunId bigint NULL
      END
            ");

            // Drop index if exists
            migrationBuilder.Sql(@"
   IF EXISTS (
   SELECT 1 
           FROM sys.indexes 
     WHERE name = 'IX_Seanslar_GunId' 
         AND object_id = OBJECT_ID('Seanslar')
      )
    BEGIN
       DROP INDEX IX_Seanslar_GunId ON Seanslar
                END
  ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate foreign key and make GunId non-nullable
            migrationBuilder.Sql(@"
      ALTER TABLE Seanslar ALTER COLUMN GunId bigint NOT NULL
    ");

            migrationBuilder.Sql(@"
ALTER TABLE Seanslar 
    ADD CONSTRAINT FK_Seanslar_Gunler_GunId 
     FOREIGN KEY (GunId) REFERENCES Gunler(Id)
   ");

            migrationBuilder.Sql(@"
    CREATE INDEX IX_Seanslar_GunId ON Seanslar(GunId)
     ");
        }
    }
}
