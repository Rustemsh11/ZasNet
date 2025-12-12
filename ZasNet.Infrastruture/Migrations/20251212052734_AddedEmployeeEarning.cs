using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmployeeEarning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecentForMultipleEmployeers",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecentLaterOrderForEmployee",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecentLaterOrderForMultipleEmployeers",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StandartPrecentForEmployee",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "EmployeeEarnings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderServiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceEmployeePrecent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecentEmployeeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmployeeEarning = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeEarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeEarnings_OrderServices_OrderServiceId",
                        column: x => x.OrderServiceId,
                        principalTable: "OrderServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEarnings_OrderServiceId",
                table: "EmployeeEarnings",
                column: "OrderServiceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeEarnings");

            migrationBuilder.DropColumn(
                name: "PrecentForMultipleEmployeers",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PrecentLaterOrderForEmployee",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PrecentLaterOrderForMultipleEmployeers",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "StandartPrecentForEmployee",
                table: "Services");
        }
    }
}
