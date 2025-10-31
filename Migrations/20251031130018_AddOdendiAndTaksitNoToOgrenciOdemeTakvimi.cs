﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOdendiAndTaksitNoToOgrenciOdemeTakvimi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Odendi",
                table: "OgrenciOdemeTakvimi",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TaksitNo",
                table: "OgrenciOdemeTakvimi",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Odendi",
                table: "OgrenciOdemeTakvimi");

            migrationBuilder.DropColumn(
                name: "TaksitNo",
                table: "OgrenciOdemeTakvimi");
        }
    }
}
