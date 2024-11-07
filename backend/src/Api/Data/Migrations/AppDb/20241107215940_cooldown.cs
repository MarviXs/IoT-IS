using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class cooldown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeactivateAfterTriggerDuration",
                table: "Scenes",
                newName: "CooldownAfterTriggerTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CooldownAfterTriggerTime",
                table: "Scenes",
                newName: "DeactivateAfterTriggerDuration");
        }
    }
}
