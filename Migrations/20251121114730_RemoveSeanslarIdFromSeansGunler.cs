using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeanslarIdFromSeansGunler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if SeanslarId column exists and drop it with all dependencies
            migrationBuilder.Sql(@"
   IF EXISTS (
           SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
   WHERE TABLE_NAME = 'SeansGunler' 
       AND COLUMN_NAME = 'SeanslarId'
      )
 BEGIN
        -- Drop index if exists
       IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SeansGunler_SeanslarId' AND object_id = OBJECT_ID('SeansGunler'))
      BEGIN
       DROP INDEX IX_SeansGunler_SeanslarId ON SeansGunler
   END
   
      -- Drop foreign key constraint if exists
      DECLARE @ConstraintName NVARCHAR(200)
 SELECT TOP 1 @ConstraintName = fk.name
     FROM sys.foreign_keys fk
 INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
     WHERE fk.parent_object_id = OBJECT_ID('SeansGunler')
         AND fk.referenced_object_id = OBJECT_ID('Seanslar')
     AND COL_NAME(fk.parent_object_id, fkc.parent_column_id) = 'SeanslarId'
       
  IF @ConstraintName IS NOT NULL
         BEGIN
     EXEC('ALTER TABLE SeansGunler DROP CONSTRAINT ' + @ConstraintName)
       END
      
       -- Drop the column
    ALTER TABLE SeansGunler DROP COLUMN SeanslarId
 END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do nothing - we don't want to recreate SeanslarId
        }
    }
}
