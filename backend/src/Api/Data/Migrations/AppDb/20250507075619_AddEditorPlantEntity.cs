using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddEditorPlantEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EditorPlants_Greenhouses_GreenHouseId1",
                table: "EditorPlants");

            migrationBuilder.DropIndex(
                name: "IX_EditorPlants_GreenHouseId1",
                table: "EditorPlants");

            migrationBuilder.DropColumn(
                name: "GreenHouseId1",
                table: "EditorPlants");

            migrationBuilder.AlterColumn<Guid>(
                name: "GreenHouseId",
                table: "EditorPlants",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9779), new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9779) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9749), new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9750) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9760), new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9760) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9771), new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9771) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9775), new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9775) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9765), new DateTime(2025, 5, 7, 7, 56, 17, 550, DateTimeKind.Utc).AddTicks(9766) });

            migrationBuilder.CreateIndex(
                name: "IX_EditorPlants_GreenHouseId",
                table: "EditorPlants",
                column: "GreenHouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_EditorPlants_Greenhouses_GreenHouseId",
                table: "EditorPlants",
                column: "GreenHouseId",
                principalTable: "Greenhouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EditorPlants_Greenhouses_GreenHouseId",
                table: "EditorPlants");

            migrationBuilder.DropIndex(
                name: "IX_EditorPlants_GreenHouseId",
                table: "EditorPlants");

            migrationBuilder.AlterColumn<string>(
                name: "GreenHouseId",
                table: "EditorPlants",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "GreenHouseId1",
                table: "EditorPlants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3761), new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3763) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3669), new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3679) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3711), new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3711) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3735), new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3736) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3754), new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3755) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3723), new DateTime(2025, 5, 6, 21, 12, 6, 328, DateTimeKind.Utc).AddTicks(3724) });

            migrationBuilder.CreateIndex(
                name: "IX_EditorPlants_GreenHouseId1",
                table: "EditorPlants",
                column: "GreenHouseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EditorPlants_Greenhouses_GreenHouseId1",
                table: "EditorPlants",
                column: "GreenHouseId1",
                principalTable: "Greenhouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
