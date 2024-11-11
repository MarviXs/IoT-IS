using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddCompanySeeder5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Invoices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "InvoiceItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(264), new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(264) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(236), new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(236) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(251), new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(251) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(258), new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(258) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(261), new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(261) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(255), new DateTime(2024, 11, 10, 22, 41, 11, 972, DateTimeKind.Utc).AddTicks(255) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "InvoiceId",
                table: "InvoiceItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6777), new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6777) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6764), new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6765) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6768), new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6768) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6773), new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6773) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6775), new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6775) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6770), new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6770) });
        }
    }
}
