using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class addedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "ChatId", "LockedAt", "LockedByUserId", "Login", "Name", "Password", "Phone", "RoleId" },
                values: new object[] { 2, null, null, null, "admin", "admin", "$2a$11$TTmUKfiEKsy8HxE2Agg2.eVxlbn/biUtN4lloHIYuqYSovk3pl5sy", null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
