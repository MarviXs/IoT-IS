using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class SetNullOnExperimentReferencesDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Experiments_Devices_DeviceId",
                table: "Experiments");

            migrationBuilder.DropForeignKey(
                name: "FK_Experiments_Jobs_RanJobId",
                table: "Experiments");

            migrationBuilder.AddForeignKey(
                name: "FK_Experiments_Devices_DeviceId",
                table: "Experiments",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Experiments_Jobs_RanJobId",
                table: "Experiments",
                column: "RanJobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Experiments_Devices_DeviceId",
                table: "Experiments");

            migrationBuilder.DropForeignKey(
                name: "FK_Experiments_Jobs_RanJobId",
                table: "Experiments");

            migrationBuilder.AddForeignKey(
                name: "FK_Experiments_Devices_DeviceId",
                table: "Experiments",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Experiments_Jobs_RanJobId",
                table: "Experiments",
                column: "RanJobId",
                principalTable: "Jobs",
                principalColumn: "Id");
        }
    }
}
