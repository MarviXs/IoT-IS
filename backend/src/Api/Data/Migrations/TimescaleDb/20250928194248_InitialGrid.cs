using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.TimescaleDb
{
    /// <inheritdoc />
    public partial class InitialGrid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GridX",
                table: "DataPoints",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GridY",
                table: "DataPoints",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GridX",
                table: "DataPoints");

            migrationBuilder.DropColumn(
                name: "GridY",
                table: "DataPoints");
        }
    }
}
