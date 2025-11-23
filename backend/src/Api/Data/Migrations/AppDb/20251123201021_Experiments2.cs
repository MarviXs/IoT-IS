using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class Experiments2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId",
                table: "Experiments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_DeviceId",
                table: "Experiments",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Experiments_Devices_DeviceId",
                table: "Experiments",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Experiments_Devices_DeviceId",
                table: "Experiments");

            migrationBuilder.DropIndex(
                name: "IX_Experiments_DeviceId",
                table: "Experiments");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Experiments");
        }
    }
}
