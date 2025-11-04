using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class ADdedCreatedUserId2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedEmployeeId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedEmployeeId",
                table: "Orders",
                column: "CreatedEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_CreatedEmployeeId",
                table: "Orders",
                column: "CreatedEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_CreatedEmployeeId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedEmployeeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedEmployeeId",
                table: "Orders");
        }
    }
}
