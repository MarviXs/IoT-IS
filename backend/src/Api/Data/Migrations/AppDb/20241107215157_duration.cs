using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class duration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeactivateAfterTriggerTime",
                table: "Scenes",
                newName: "DeactivateAfterTriggerDuration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeactivateAfterTriggerDuration",
                table: "Scenes",
                newName: "DeactivateAfterTriggerTime");
        }
    }
}
