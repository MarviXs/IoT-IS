using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class ControlTypeToggle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceControls_Recipes_RecipeId",
                table: "DeviceControls");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecipeId",
                table: "DeviceControls",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "RecipeOffId",
                table: "DeviceControls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipeOnId",
                table: "DeviceControls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SensorId",
                table: "DeviceControls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "DeviceControls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceControls_RecipeOffId",
                table: "DeviceControls",
                column: "RecipeOffId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceControls_RecipeOnId",
                table: "DeviceControls",
                column: "RecipeOnId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceControls_SensorId",
                table: "DeviceControls",
                column: "SensorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceControls_Recipes_RecipeId",
                table: "DeviceControls",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceControls_Recipes_RecipeOffId",
                table: "DeviceControls",
                column: "RecipeOffId",
                principalTable: "Recipes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceControls_Recipes_RecipeOnId",
                table: "DeviceControls",
                column: "RecipeOnId",
                principalTable: "Recipes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceControls_Sensors_SensorId",
                table: "DeviceControls",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceControls_Recipes_RecipeId",
                table: "DeviceControls");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceControls_Recipes_RecipeOffId",
                table: "DeviceControls");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceControls_Recipes_RecipeOnId",
                table: "DeviceControls");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceControls_Sensors_SensorId",
                table: "DeviceControls");

            migrationBuilder.DropIndex(
                name: "IX_DeviceControls_RecipeOffId",
                table: "DeviceControls");

            migrationBuilder.DropIndex(
                name: "IX_DeviceControls_RecipeOnId",
                table: "DeviceControls");

            migrationBuilder.DropIndex(
                name: "IX_DeviceControls_SensorId",
                table: "DeviceControls");

            migrationBuilder.DropColumn(
                name: "RecipeOffId",
                table: "DeviceControls");

            migrationBuilder.DropColumn(
                name: "RecipeOnId",
                table: "DeviceControls");

            migrationBuilder.DropColumn(
                name: "SensorId",
                table: "DeviceControls");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DeviceControls");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecipeId",
                table: "DeviceControls",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceControls_Recipes_RecipeId",
                table: "DeviceControls",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
