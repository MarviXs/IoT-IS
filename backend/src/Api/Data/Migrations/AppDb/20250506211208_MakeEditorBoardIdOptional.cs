using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class MakeEditorBoardIdOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
