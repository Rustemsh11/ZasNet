using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class AddedDispetcherProcent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DispetcherProcent",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                column: "DispetcherProcent",
                value: null);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                column: "DispetcherProcent",
                value: 2m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DispetcherProcent",
                table: "Employees");
        }
    }
}
