using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class RemoveShared : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceShares_AspNetUsers_SharedToUserId",
                table: "DeviceShares");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceShares_AspNetUsers_SharingUserId",
                table: "DeviceShares");

            migrationBuilder.DropIndex(
                name: "IX_DeviceShares_SharingUserId",
                table: "DeviceShares");

            migrationBuilder.DropColumn(
                name: "SharingUserId",
                table: "DeviceShares");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceShares_AspNetUsers_SharedToUserId",
                table: "DeviceShares",
                column: "SharedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceShares_AspNetUsers_SharedToUserId",
                table: "DeviceShares");

            migrationBuilder.AddColumn<Guid>(
                name: "SharingUserId",
                table: "DeviceShares",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DeviceShares_SharingUserId",
                table: "DeviceShares",
                column: "SharingUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceShares_AspNetUsers_SharedToUserId",
                table: "DeviceShares",
                column: "SharedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceShares_AspNetUsers_SharingUserId",
                table: "DeviceShares",
                column: "SharingUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
