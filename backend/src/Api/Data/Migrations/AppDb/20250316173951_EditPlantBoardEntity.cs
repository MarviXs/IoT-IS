using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class EditPlantBoardEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PlantBoardId1",
                table: "Plants",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "PlantBoardId",
                table: "Plants",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "PlantBoardId",
                table: "PlantBoards",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8798), new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8798) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8783), new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8784) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8789), new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8789) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8794), new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8794) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8796), new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8796) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8791), new DateTime(2025, 3, 16, 17, 39, 51, 77, DateTimeKind.Utc).AddTicks(8792) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PlantBoardId1",
                table: "Plants",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlantBoardId",
                table: "Plants",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlantBoardId",
                table: "PlantBoards",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

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
        }
    }
}
