using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class Companies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ulice",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "Title2",
                table: "Companies",
                newName: "Street");

            migrationBuilder.AlterColumn<string>(
                name: "Psc",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Dic",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "Street", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Blešno 127", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "Street", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Třemošenská 658", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "Street", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "K Vršíčku 91", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "Street", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Olbramkostel 41", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "Street", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mírová 407", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "Street", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lhota 244", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Companies",
                newName: "Title2");

            migrationBuilder.AlterColumn<string>(
                name: "Psc",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Dic",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ulice",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                columns: new[] { "CreatedAt", "Title2", "Ulice", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5455), null, "Blešno 127", new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5455) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"),
                columns: new[] { "CreatedAt", "Title2", "Ulice", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5442), null, "Třemošenská 658", new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5442) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                columns: new[] { "CreatedAt", "Title2", "Ulice", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5445), null, "K Vršíčku 91", new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5445) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"),
                columns: new[] { "CreatedAt", "Title2", "Ulice", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5451), null, "Olbramkostel 41", new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5451) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                columns: new[] { "CreatedAt", "Title2", "Ulice", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5453), null, "Mírová 407", new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5453) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                columns: new[] { "CreatedAt", "Title2", "Ulice", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5449), null, "Lhota 244", new DateTime(2024, 12, 14, 17, 52, 22, 27, DateTimeKind.Utc).AddTicks(5449) });
        }
    }
}
