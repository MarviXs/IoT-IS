using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class FixPlantBoardRelation3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plants_PlantBoards_PlantBoardId1",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_PlantBoardId1",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PlantBoardId1",
                table: "Plants");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8052), new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8052) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8040), new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8040) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8043), new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8043) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8048), new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8048) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8050), new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8050) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8046), new DateTime(2025, 3, 17, 20, 14, 53, 804, DateTimeKind.Utc).AddTicks(8046) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlantBoardId1",
                table: "Plants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9321), new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9321) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9308), new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9309) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9313), new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9313) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9318), new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9318) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9320), new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9320) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9315), new DateTime(2025, 3, 17, 20, 10, 5, 392, DateTimeKind.Utc).AddTicks(9316) });

            migrationBuilder.CreateIndex(
                name: "IX_Plants_PlantBoardId1",
                table: "Plants",
                column: "PlantBoardId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_PlantBoards_PlantBoardId1",
                table: "Plants",
                column: "PlantBoardId1",
                principalTable: "PlantBoards",
                principalColumn: "PlantBoardId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
