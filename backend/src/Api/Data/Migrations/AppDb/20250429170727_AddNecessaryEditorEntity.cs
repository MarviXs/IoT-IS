using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddNecessaryEditorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EditorPots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EditorBoardID = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Columns = table.Column<int>(type: "integer", nullable: false),
                    Rows = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    PosX = table.Column<int>(type: "integer", nullable: false),
                    PosY = table.Column<int>(type: "integer", nullable: false),
                    Shape = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GreenHouseId = table.Column<string>(type: "text", nullable: false),
                    GreenHouseId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorPots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorPots_Greenhouses_GreenHouseId1",
                        column: x => x.GreenHouseId1,
                        principalTable: "Greenhouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditorPlants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantID = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    PosX = table.Column<int>(type: "integer", nullable: false),
                    PosY = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentDay = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    CurrentState = table.Column<string>(type: "text", nullable: false),
                    EditorBoardId = table.Column<string>(type: "text", nullable: false),
                    GreenHouseId = table.Column<string>(type: "text", nullable: false),
                    GreenHouseId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    EditorBoardId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorPlants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorPlants_EditorPots_EditorBoardId1",
                        column: x => x.EditorBoardId1,
                        principalTable: "EditorPots",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EditorPlants_Greenhouses_GreenHouseId1",
                        column: x => x.GreenHouseId1,
                        principalTable: "Greenhouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4104), new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4105) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4089), new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4089) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4094), new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4094) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4100), new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4100) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4102), new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4102) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4097), new DateTime(2025, 4, 29, 17, 7, 27, 241, DateTimeKind.Utc).AddTicks(4097) });

            migrationBuilder.CreateIndex(
                name: "IX_EditorPlants_EditorBoardId1",
                table: "EditorPlants",
                column: "EditorBoardId1");

            migrationBuilder.CreateIndex(
                name: "IX_EditorPlants_GreenHouseId1",
                table: "EditorPlants",
                column: "GreenHouseId1");

            migrationBuilder.CreateIndex(
                name: "IX_EditorPots_GreenHouseId1",
                table: "EditorPots",
                column: "GreenHouseId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EditorPlants");

            migrationBuilder.DropTable(
                name: "EditorPots");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7764), new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7764) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7750), new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7750) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7754), new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7754) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7759), new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7760) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7762), new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7762) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7757), new DateTime(2025, 4, 29, 10, 57, 7, 541, DateTimeKind.Utc).AddTicks(7757) });
        }
    }
}
