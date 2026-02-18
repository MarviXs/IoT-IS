using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class nodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SyncedFromEdge",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncedFromEdgeNodeId",
                table: "Devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SyncedFromEdge",
                table: "DeviceTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncedFromEdgeNodeId",
                table: "DeviceTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EdgeNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdateRateSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdgeNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemNodeSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HubUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    HubToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SyncIntervalSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemNodeSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SyncedFromEdgeNodeId",
                table: "Devices",
                column: "SyncedFromEdgeNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTemplates_SyncedFromEdgeNodeId",
                table: "DeviceTemplates",
                column: "SyncedFromEdgeNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_EdgeNodes_Token",
                table: "EdgeNodes",
                column: "Token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTemplates_EdgeNodes_SyncedFromEdgeNodeId",
                table: "DeviceTemplates",
                column: "SyncedFromEdgeNodeId",
                principalTable: "EdgeNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_EdgeNodes_SyncedFromEdgeNodeId",
                table: "Devices",
                column: "SyncedFromEdgeNodeId",
                principalTable: "EdgeNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTemplates_EdgeNodes_SyncedFromEdgeNodeId",
                table: "DeviceTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_EdgeNodes_SyncedFromEdgeNodeId",
                table: "Devices");

            migrationBuilder.DropTable(
                name: "EdgeNodes");

            migrationBuilder.DropTable(
                name: "SystemNodeSettings");

            migrationBuilder.DropIndex(
                name: "IX_Devices_SyncedFromEdgeNodeId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTemplates_SyncedFromEdgeNodeId",
                table: "DeviceTemplates");

            migrationBuilder.DropColumn(
                name: "SyncedFromEdge",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SyncedFromEdgeNodeId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SyncedFromEdge",
                table: "DeviceTemplates");

            migrationBuilder.DropColumn(
                name: "SyncedFromEdgeNodeId",
                table: "DeviceTemplates");
        }
    }
}
