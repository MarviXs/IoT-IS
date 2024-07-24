using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class templateConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceTemplates_DeviceTemplateId",
                table: "Devices");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceTemplateId",
                table: "Devices",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTemplates_Name",
                table: "DeviceTemplates",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceTemplates_DeviceTemplateId",
                table: "Devices",
                column: "DeviceTemplateId",
                principalTable: "DeviceTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceTemplates_DeviceTemplateId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTemplates_Name",
                table: "DeviceTemplates");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeviceTemplateId",
                table: "Devices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceTemplates_DeviceTemplateId",
                table: "Devices",
                column: "DeviceTemplateId",
                principalTable: "DeviceTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
