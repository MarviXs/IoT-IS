using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class SceneOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Scenes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_OwnerId",
                table: "Scenes",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scenes_AspNetUsers_OwnerId",
                table: "Scenes",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scenes_AspNetUsers_OwnerId",
                table: "Scenes");

            migrationBuilder.DropIndex(
                name: "IX_Scenes_OwnerId",
                table: "Scenes");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Scenes");
        }
    }
}
