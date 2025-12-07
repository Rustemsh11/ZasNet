using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ZasNet.Infrastruture.Migrations
{
    /// <inheritdoc />
    public partial class addedMeasure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Measure",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "MeasureId",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Measures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measures", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Measures",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "метр" },
                    { 2, "час" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_MeasureId",
                table: "Services",
                column: "MeasureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Measures_MeasureId",
                table: "Services",
                column: "MeasureId",
                principalTable: "Measures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Measures_MeasureId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "Measures");

            migrationBuilder.DropIndex(
                name: "IX_Services_MeasureId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "MeasureId",
                table: "Services");

            migrationBuilder.AddColumn<string>(
                name: "Measure",
                table: "Services",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
