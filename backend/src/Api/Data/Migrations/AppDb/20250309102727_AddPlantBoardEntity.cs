using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddPlantBoardEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlantBoardId",
                table: "Plants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PlantBoardId1",
                table: "Plants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PlantBoards",
                columns: table => new
                {
                    PlantBoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rows = table.Column<int>(type: "integer", nullable: false),
                    Cols = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantBoards", x => x.PlantBoardId);
                });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1050), new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1050) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1036), new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1036) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1040), new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1040) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1046), new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1046) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1048), new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1048) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1043), new DateTime(2025, 3, 9, 10, 27, 26, 423, DateTimeKind.Utc).AddTicks(1043) });

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

            migrationBuilder.DropTable(
                name: "PlantBoards");

            migrationBuilder.DropIndex(
                name: "IX_Plants_PlantBoardId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_PlantBoardId1",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PlantBoardId",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PlantBoardId1",
                table: "Plants");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4718), new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4719) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4705), new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4706) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4710), new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4710) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4715), new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4715) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4717), new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4717) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4713), new DateTime(2025, 1, 26, 15, 50, 36, 539, DateTimeKind.Utc).AddTicks(4713) });
        }
    }
}
