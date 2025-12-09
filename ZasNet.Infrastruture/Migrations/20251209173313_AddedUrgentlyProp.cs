using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class AddedUrgentlyProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedInvoiceUrgently",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "LockedAt", "LockedByUserId", "Name" },
                values: new object[] { 4, null, null, "Бухгалтер" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "NeedInvoiceUrgently",
                table: "Orders");
        }
    }
}
