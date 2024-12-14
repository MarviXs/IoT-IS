using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class SeedingFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6bf2fd3c-1185-47c4-870f-32738d045f36"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7905728d-ce7d-486b-a981-2882232f1b6b"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("412ceb2b-ca6a-43c9-80e1-6eb1cb16164a"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("4fd1cbf4-bef4-4fee-b72f-fac1b15c8357"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "VATCategories",
                keyColumn: "Id",
                keyValue: new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "VATCategories",
                keyColumn: "Id",
                keyValue: new Guid("5bfc3ed5-8874-4452-9043-22065fc00e29"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6bf2fd3c-1185-47c4-870f-32738d045f36"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2263), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2265) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7905728d-ce7d-486b-a981-2882232f1b6b"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2274), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2274) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("412ceb2b-ca6a-43c9-80e1-6eb1cb16164a"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2353), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2353) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("4fd1cbf4-bef4-4fee-b72f-fac1b15c8357"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2349), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2350) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2352), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2352) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2350), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2351) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2347), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2347) });

            migrationBuilder.UpdateData(
                table: "VATCategories",
                keyColumn: "Id",
                keyValue: new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2418), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2419) });

            migrationBuilder.UpdateData(
                table: "VATCategories",
                keyColumn: "Id",
                keyValue: new Guid("5bfc3ed5-8874-4452-9043-22065fc00e29"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2415), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2415) });
        }
    }
}
