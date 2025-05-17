using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEditBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EditorPots_Greenhouses_GreenHouseId1",
                table: "EditorPots");

            migrationBuilder.DropIndex(
                name: "IX_EditorPots_GreenHouseId1",
                table: "EditorPots");

            migrationBuilder.DropColumn(
                name: "GreenHouseId1",
                table: "EditorPots");

            // Opravená typová konverzia
            migrationBuilder.Sql(
                "ALTER TABLE \"EditorPots\" ALTER COLUMN \"GreenHouseId\" TYPE uuid USING \"GreenHouseId\"::uuid;");

            migrationBuilder.CreateIndex(
                name: "IX_EditorPots_GreenHouseId",
                table: "EditorPots",
                column: "GreenHouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_EditorPots_Greenhouses_GreenHouseId",
                table: "EditorPots",
                column: "GreenHouseId",
                principalTable: "Greenhouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EditorPots_Greenhouses_GreenHouseId",
                table: "EditorPots");

            migrationBuilder.DropIndex(
                name: "IX_EditorPots_GreenHouseId",
                table: "EditorPots");

            migrationBuilder.AlterColumn<string>(
                name: "GreenHouseId",
                table: "EditorPots",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "GreenHouseId1",
                table: "EditorPots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_EditorPots_GreenHouseId1",
                table: "EditorPots",
                column: "GreenHouseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EditorPots_Greenhouses_GreenHouseId1",
                table: "EditorPots",
                column: "GreenHouseId1",
                principalTable: "Greenhouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
