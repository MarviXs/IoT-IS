using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class Experiments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Experiments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    RecipeToRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    RanJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experiments_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Experiments_Jobs_RanJobId",
                        column: x => x.RanJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Experiments_Recipes_RecipeToRunId",
                        column: x => x.RecipeToRunId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_OwnerId",
                table: "Experiments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_RanJobId",
                table: "Experiments",
                column: "RanJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_RecipeToRunId",
                table: "Experiments",
                column: "RecipeToRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Experiments");
        }
    }
}
