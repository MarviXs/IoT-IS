using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class EdgeNodeUpdateRateDefaultFive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UpdateRateSeconds",
                table: "EdgeNodes",
                type: "integer",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 60);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UpdateRateSeconds",
                table: "EdgeNodes",
                type: "integer",
                nullable: false,
                defaultValue: 60,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 5);
        }
    }
}
