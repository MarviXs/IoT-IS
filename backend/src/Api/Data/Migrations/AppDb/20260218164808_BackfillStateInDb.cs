using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class BackfillStateInDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BackfillCompleted",
                table: "SystemNodeSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BackfillCursorOffset",
                table: "SystemNodeSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "BackfillCursorTimestampUnixMs",
                table: "SystemNodeSettings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BackfillCutoffUnixMs",
                table: "SystemNodeSettings",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackfillCompleted",
                table: "SystemNodeSettings");

            migrationBuilder.DropColumn(
                name: "BackfillCursorOffset",
                table: "SystemNodeSettings");

            migrationBuilder.DropColumn(
                name: "BackfillCursorTimestampUnixMs",
                table: "SystemNodeSettings");

            migrationBuilder.DropColumn(
                name: "BackfillCutoffUnixMs",
                table: "SystemNodeSettings");
        }
    }
}
