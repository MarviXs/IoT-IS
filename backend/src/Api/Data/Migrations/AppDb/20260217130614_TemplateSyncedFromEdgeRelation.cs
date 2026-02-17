using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class TemplateSyncedFromEdgeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSyncedFromHub",
                table: "DeviceTemplates",
                newName: "SyncedFromEdge");

            migrationBuilder.AddColumn<Guid>(
                name: "SyncedFromEdgeNodeId",
                table: "DeviceTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTemplates_SyncedFromEdgeNodeId",
                table: "DeviceTemplates",
                column: "SyncedFromEdgeNodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTemplates_EdgeNodes_SyncedFromEdgeNodeId",
                table: "DeviceTemplates",
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

            migrationBuilder.DropIndex(
                name: "IX_DeviceTemplates_SyncedFromEdgeNodeId",
                table: "DeviceTemplates");

            migrationBuilder.DropColumn(
                name: "SyncedFromEdgeNodeId",
                table: "DeviceTemplates");

            migrationBuilder.RenameColumn(
                name: "SyncedFromEdge",
                table: "DeviceTemplates",
                newName: "IsSyncedFromHub");
        }
    }
}
