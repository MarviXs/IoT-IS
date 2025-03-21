using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class FixPlantBoardRelation11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plants_PlantBoards_Id",
                table: "Plants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlantBoards",
                table: "PlantBoards");

            migrationBuilder.AlterColumn<string>(
                name: "PlantBoardId",
                table: "Plants",
                type: "character varying(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "PlantBoardId1",
                table: "Plants",
                type: "character varying(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlantBoards",
                table: "PlantBoards",
                column: "PlantBoardId");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6125), new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6125) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6110), new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6111) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6115), new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6115) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6121), new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6121) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6123), new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6123) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6118), new DateTime(2025, 3, 17, 21, 30, 33, 272, DateTimeKind.Utc).AddTicks(6118) });

            migrationBuilder.CreateIndex(
                name: "IX_Plants_PlantBoardId",
                table: "Plants",
                column: "PlantBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_PlantBoardId1",
                table: "Plants",
                column: "PlantBoardId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_PlantBoards_PlantBoardId",
                table: "Plants",
                column: "PlantBoardId",
                principalTable: "PlantBoards",
                principalColumn: "PlantBoardId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_PlantBoards_PlantBoardId1",
                table: "Plants",
                column: "PlantBoardId1",
                principalTable: "PlantBoards",
                principalColumn: "PlantBoardId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plants_PlantBoards_PlantBoardId",
                table: "Plants");

            migrationBuilder.DropForeignKey(
                name: "FK_Plants_PlantBoards_PlantBoardId1",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_PlantBoardId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_PlantBoardId1",
                table: "Plants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlantBoards",
                table: "PlantBoards");

            migrationBuilder.DropColumn(
                name: "PlantBoardId1",
                table: "Plants");

            migrationBuilder.AlterColumn<string>(
                name: "PlantBoardId",
                table: "Plants",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlantBoards",
                table: "PlantBoards",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6410), new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6410) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6394), new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6395) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6400), new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6400) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6406), new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6406) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6408), new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6408) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6403), new DateTime(2025, 3, 17, 21, 23, 48, 455, DateTimeKind.Utc).AddTicks(6403) });

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_PlantBoards_Id",
                table: "Plants",
                column: "Id",
                principalTable: "PlantBoards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
