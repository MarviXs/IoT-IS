using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class RemoveJobStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobStatuses");

            migrationBuilder.DropColumn(
                name: "ToCancel",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "NoOfReps",
                table: "Jobs",
                newName: "TotalSteps");

            migrationBuilder.RenameColumn(
                name: "NoOfCmds",
                table: "Jobs",
                newName: "TotalCycles");

            migrationBuilder.AddColumn<int>(
                name: "CurrentCycle",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCycle",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "TotalSteps",
                table: "Jobs",
                newName: "NoOfReps");

            migrationBuilder.RenameColumn(
                name: "TotalCycles",
                table: "Jobs",
                newName: "NoOfCmds");

            migrationBuilder.AddColumn<bool>(
                name: "ToCancel",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "JobStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentCycle = table.Column<int>(type: "integer", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    RetCode = table.Column<int>(type: "integer", nullable: false),
                    TotalSteps = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobStatuses_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Npgsql:StorageParameter:fillfactor", 70);

            migrationBuilder.CreateIndex(
                name: "IX_JobStatuses_JobId",
                table: "JobStatuses",
                column: "JobId",
                unique: true);
        }
    }
}
