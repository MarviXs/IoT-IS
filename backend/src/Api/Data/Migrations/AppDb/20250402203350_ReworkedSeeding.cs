using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class ReworkedSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6bf2fd3c-1185-47c4-870f-32738d045f36"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7905728d-ce7d-486b-a981-2882232f1b6b"));

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

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("412ceb2b-ca6a-43c9-80e1-6eb1cb16164a"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("4fd1cbf4-bef4-4fee-b72f-fac1b15c8357"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"));

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"));

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "DocumentTemplatesLoadPath");

            migrationBuilder.DeleteData(
                table: "VATCategories",
                keyColumn: "Id",
                keyValue: new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"));

            migrationBuilder.DeleteData(
                table: "VATCategories",
                keyColumn: "Id",
                keyValue: new Guid("5bfc3ed5-8874-4452-9043-22065fc00e29"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CategoryName", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("6bf2fd3c-1185-47c4-870f-32738d045f36"), "Nejaka burina", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("7905728d-ce7d-486b-a981-2882232f1b6b"), "Nejaky strom", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "City", "CreatedAt", "Dic", "Ic", "Psc", "Street", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("3b29e227-bb97-441f-878e-d4b1111b3ebb"), "Třebechovice pod Orebem", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CZ6203071741", "46212152", "50346", "Blešno 127", "Jan Zatloukal - Zahradnictví Blešno", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("479b6c63-f552-4a6e-b706-62ec96edb896"), "Příbram", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CZ25735641", "25735641", "26101", "Třemošenská 658", "Petunia s.r.o.", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"), "Stará Huť", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CZ696123003", "71070877", "26202", "K Vršíčku 91", "Štamberková Monika", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("91827187-f264-44b2-b6e3-697a752aa968"), "Olbramkostel", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CZ28282711", "28282711", "67151", "Olbramkostel 41", "Moravol s.r.o.", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("be97065a-c6e9-4b03-a173-c1c85f9b42db"), "Horní Benešov", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CZ29296721", "29296721", "79312", "Mírová 407", "LM Agroton s.r.o.", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("f51a5725-b267-4c19-9cf0-444bb7c32b6e"), "Dřísy", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CZ27469613", "27469613", "27714", "Lhota 244", "Arboeko s.r.o.", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("412ceb2b-ca6a-43c9-80e1-6eb1cb16164a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Internal", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("4fd1cbf4-bef4-4fee-b72f-fac1b15c8357"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bennials", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Syngenta", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Schneider", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Volmary", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Key", "Value" },
                values: new object[] { "DocumentTemplatesLoadPath", "./DocumentTemplates" });

            migrationBuilder.InsertData(
                table: "VATCategories",
                columns: new[] { "Id", "CreatedAt", "Name", "Rate", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reduced", 19m, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("5bfc3ed5-8874-4452-9043-22065fc00e29"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Normal", 21m, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }
    }
}
