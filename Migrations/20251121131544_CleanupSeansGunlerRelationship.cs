using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class CleanupSeansGunlerRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if constraint exists before dropping
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SeansGunler_Seanslar_SeanslarId')
     ALTER TABLE [SeansGunler] DROP CONSTRAINT [FK_SeansGunler_Seanslar_SeanslarId];
 ");

            migrationBuilder.Sql(@"
             IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Seanslar_Gunler_GunId')
 ALTER TABLE [Seanslar] DROP CONSTRAINT [FK_Seanslar_Gunler_GunId];
   ");

   migrationBuilder.Sql(@"
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Seanslar_GunId' AND object_id = OBJECT_ID('Seanslar'))
       DROP INDEX [IX_Seanslar_GunId] ON [Seanslar];
  ");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SeansGunler_SeanslarId' AND object_id = OBJECT_ID('SeansGunler'))
    DROP INDEX [IX_SeansGunler_SeanslarId] ON [SeansGunler];
            ");

      migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'GunId' AND object_id = OBJECT_ID('Seanslar'))
        ALTER TABLE [Seanslar] DROP COLUMN [GunId];
 ");

            migrationBuilder.Sql(@"
      IF EXISTS (SELECT * FROM sys.columns WHERE name = 'SeanslarId' AND object_id = OBJECT_ID('SeansGunler'))
          ALTER TABLE [SeansGunler] DROP COLUMN [SeanslarId];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.AddColumn<long>(
        name: "GunId",
      table: "Seanslar",
        type: "bigint",
      nullable: true);

      migrationBuilder.AddColumn<long>(
     name: "SeanslarId",
       table: "SeansGunler",
      type: "bigint",
     nullable: true);

        migrationBuilder.CreateIndex(
      name: "IX_Seanslar_GunId",
  table: "Seanslar",
      column: "GunId");

       migrationBuilder.CreateIndex(
      name: "IX_SeansGunler_SeanslarId",
      table: "SeansGunler",
     column: "SeanslarId");

  migrationBuilder.AddForeignKey(
          name: "FK_SeansGunler_Seanslar_SeanslarId",
     table: "SeansGunler",
   column: "SeanslarId",
   principalTable: "Seanslar",
       principalColumn: "Id");

            migrationBuilder.AddForeignKey(
           name: "FK_Seanslar_Gunler_GunId",
                table: "Seanslar",
    column: "GunId",
        principalTable: "Gunler",
     principalColumn: "Id",
  onDelete: ReferentialAction.Restrict);
    }
    }
}
