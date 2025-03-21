using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class FixPlantBoardRelation4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plants_PlantBoards_PlantBoardId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_PlantBoardId",
                table: "Plants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlantBoards",
                table: "PlantBoards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlantBoards",
                table: "PlantBoards",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9372), new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9373) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9362), new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9362) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9365), new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9365) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9369), new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9369) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9371), new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9371) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9367), new DateTime(2025, 3, 17, 20, 17, 59, 901, DateTimeKind.Utc).AddTicks(9368) });

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_PlantBoards_Id",
                table: "Plants",
                column: "Id",
                principalTable: "PlantBoards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plants_PlantBoards_Id",
                table: "Plants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlantBoards",
                table: "PlantBoards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlantBoards",
                table: "PlantBoards",
                column: "PlantBoardId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Plants_PlantBoardId",
                table: "Plants",
                column: "PlantBoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_PlantBoards_PlantBoardId",
                table: "Plants",
                column: "PlantBoardId",
                principalTable: "PlantBoards",
                principalColumn: "PlantBoardId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
