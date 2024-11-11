using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddCompanySeeder4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplierId",
                table: "WorkReports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "WorkReports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "WorkReports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "WorkReports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplierId",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Invoices",
                //type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "InvoiceId",
                table: "InvoiceItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "InvoiceItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "InvoiceItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InvoiceItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplierId",
                table: "DeliveryNotes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "DeliveryNotes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DeliveryNotes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryNoteId",
                table: "DeliveryItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Companies",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "City", "CreatedAt", "Dic", "Ic", "Psc", "Title", "Title2", "Ulice", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"), "Třebechovice pod Orebem", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6777), "CZ6203071741", "46212152", "50346", "Jan Zatloukal - Zahradnictví Blešno", null, "Blešno 127", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6777) },
                    { new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"), "Příbram", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6764), "CZ25735641", "25735641", "26101", "Petunia s.r.o.", null, "Třemošenská 658", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6765) },
                    { new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"), "Stará Huť", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6768), "CZ696123003", "71070877", "26202", "Štamberková Monika", null, "K Vršíčku 91", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6768) },
                    { new Guid("91827187-f264-44b2-b6e3-697a752aa968"), "Olbramkostel", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6773), "CZ28282711", "28282711", "67151", "Moravol s.r.o.", null, "Olbramkostel 41", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6773) },
                    { new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"), "Horní Benešov", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6775), "CZ29296721", "29296721", "79312", "LM Agroton s.r.o.", null, "Mírová 407", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6775) },
                    { new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"), "Dřísy", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6770), "CZ27469613", "27469613", "27714", "Arboeko s.r.o.", null, "Lhota 244", new DateTime(2024, 11, 10, 22, 21, 50, 746, DateTimeKind.Utc).AddTicks(6770) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("91827187-f264-44b2-b6e3-697a752aa968"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"));

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "WorkReports");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WorkReports");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InvoiceItems");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "WorkReports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "WorkReports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "Invoices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Invoices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "InvoiceItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "DeliveryNotes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "DeliveryNotes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DeliveryNotes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryNoteId",
                table: "DeliveryItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Companies",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "City", "CreatedAt", "Dic", "Ic", "Psc", "Title", "Title2", "Ulice", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Příbram", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1371), "CZ25735641", "25735641", "26101", "Petunia s.r.o.", null, "Třemošenská 658", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1372) },
                    { 2, "Stará Huť", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1416), "CZ696123003", "71070877", "26202", "Štamberková Monika", null, "K Vršíčku 91", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1416) },
                    { 3, "Dřísy", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1418), "CZ27469613", "27469613", "27714", "Arboeko s.r.o.", null, "Lhota 244", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1418) },
                    { 4, "Olbramkostel", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1420), "CZ28282711", "28282711", "67151", "Moravol s.r.o.", null, "Olbramkostel 41", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1420) },
                    { 5, "Horní Benešov", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1421), "CZ29296721", "29296721", "79312", "LM Agroton s.r.o.", null, "Mírová 407", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1422) },
                    { 6, "Třebechovice pod Orebem", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1423), "CZ6203071741", "46212152", "50346", "Jan Zatloukal - Zahradnictví Blešno", null, "Blešno 127", new DateTime(2024, 11, 10, 21, 58, 42, 10, DateTimeKind.Utc).AddTicks(1423) }
                });
        }
    }
}
