using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class DispEarning2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DispetcherEarnings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ServiceEmployeePrecent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecentEmployeeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmployeeEarning = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispetcherEarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispetcherEarnings_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispetcherEarnings_OrderId",
                table: "DispetcherEarnings",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispetcherEarnings");
        }
    }
}
