using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderCarConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderPerformCars_Cars_CarId",
                table: "OrderPerformCars");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderPerformCars_Orders_OrderId",
                table: "OrderPerformCars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderPerformCars",
                table: "OrderPerformCars");

            migrationBuilder.RenameTable(
                name: "OrderPerformCars",
                newName: "OrderCars");

            migrationBuilder.RenameIndex(
                name: "IX_OrderPerformCars_OrderId",
                table: "OrderCars",
                newName: "IX_OrderCars_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderPerformCars_CarId",
                table: "OrderCars",
                newName: "IX_OrderCars_CarId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderCars",
                table: "OrderCars",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderCars_Cars_CarId",
                table: "OrderCars",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderCars_Orders_OrderId",
                table: "OrderCars",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderCars_Cars_CarId",
                table: "OrderCars");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderCars_Orders_OrderId",
                table: "OrderCars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderCars",
                table: "OrderCars");

            migrationBuilder.RenameTable(
                name: "OrderCars",
                newName: "OrderPerformCars");

            migrationBuilder.RenameIndex(
                name: "IX_OrderCars_OrderId",
                table: "OrderPerformCars",
                newName: "IX_OrderPerformCars_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderCars_CarId",
                table: "OrderPerformCars",
                newName: "IX_OrderPerformCars_CarId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderPerformCars",
                table: "OrderPerformCars",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPerformCars_Cars_CarId",
                table: "OrderPerformCars",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPerformCars_Orders_OrderId",
                table: "OrderPerformCars",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
