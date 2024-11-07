using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class RenameTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SceneSensorActivators");

            migrationBuilder.AddColumn<long>(
                name: "DeactivateAfterTriggerTime",
                table: "Scenes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "SceneSensorTriggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SceneId = table.Column<Guid>(type: "uuid", nullable: false),
                    SensorTag = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SceneSensorTriggers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SceneSensorTriggers_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SceneSensorTriggers_Scenes_SceneId",
                        column: x => x.SceneId,
                        principalTable: "Scenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SceneSensorTriggers_DeviceId",
                table: "SceneSensorTriggers",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SceneSensorTriggers_SceneId_DeviceId_SensorTag",
                table: "SceneSensorTriggers",
                columns: new[] { "SceneId", "DeviceId", "SensorTag" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SceneSensorTriggers");

            migrationBuilder.DropColumn(
                name: "DeactivateAfterTriggerTime",
                table: "Scenes");

            migrationBuilder.CreateTable(
                name: "SceneSensorActivators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SceneId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SensorTag = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SceneSensorActivators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SceneSensorActivators_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SceneSensorActivators_Scenes_SceneId",
                        column: x => x.SceneId,
                        principalTable: "Scenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SceneSensorActivators_DeviceId",
                table: "SceneSensorActivators",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SceneSensorActivators_SceneId_DeviceId_SensorTag",
                table: "SceneSensorActivators",
                columns: new[] { "SceneId", "DeviceId", "SensorTag" },
                unique: true);
        }
    }
}
