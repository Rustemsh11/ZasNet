using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class movedIsAlmazService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlmazOrder",
                table: "Orders");

            migrationBuilder.AddColumn<bool>(
                name: "IsAlmazService",
                table: "OrderServices",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlmazService",
                table: "OrderServices");

            migrationBuilder.AddColumn<bool>(
                name: "IsAlmazOrder",
                table: "Orders",
                type: "bit",
                nullable: true);
        }
    }
}
