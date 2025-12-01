using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class RefactoredByRequirenments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Orders",
                newName: "DateStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEnd",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateEnd",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "DateStart",
                table: "Orders",
                newName: "Date");

            migrationBuilder.AddColumn<short>(
                name: "ClientType",
                table: "Orders",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
