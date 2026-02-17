using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class DeviceSyncedFromEdgeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSyncedFromHub",
                table: "Devices",
                newName: "SyncedFromEdge");

            migrationBuilder.AddColumn<Guid>(
                name: "SyncedFromEdgeNodeId",
                table: "Devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SyncedFromEdgeNodeId",
                table: "Devices",
                column: "SyncedFromEdgeNodeId");

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
                name: "FK_Devices_EdgeNodes_SyncedFromEdgeNodeId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_SyncedFromEdgeNodeId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SyncedFromEdgeNodeId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "SyncedFromEdge",
                table: "Devices",
                newName: "IsSyncedFromHub");
        }
    }
}
