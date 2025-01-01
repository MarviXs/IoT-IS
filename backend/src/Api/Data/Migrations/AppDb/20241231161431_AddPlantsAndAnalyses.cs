using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddPlantsAndAnalyses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DatePlanted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlantAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Height = table.Column<double>(type: "double precision", nullable: false),
                    Width = table.Column<double>(type: "double precision", nullable: false),
                    LeafCount = table.Column<int>(type: "integer", nullable: false),
                    Area = table.Column<double>(type: "double precision", nullable: false),
                    Disease = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Health = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantAnalyses_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4547), new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4547) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4532), new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4532) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4537), new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4537) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4543), new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4543) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4545), new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4545) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4540), new DateTime(2024, 12, 31, 16, 14, 30, 724, DateTimeKind.Utc).AddTicks(4540) });

            migrationBuilder.CreateIndex(
                name: "IX_PlantAnalyses_PlantId",
                table: "PlantAnalyses",
                column: "PlantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantAnalyses");

            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5455), new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5455) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5442), new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5442) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5445), new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5445) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5451), new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5451) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5453), new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5453) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5449), new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5449) });
        }
    }
}
