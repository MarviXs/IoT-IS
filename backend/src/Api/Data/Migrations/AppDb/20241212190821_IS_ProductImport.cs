using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class IS_ProductImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("0c1d6c90-4937-4b87-b8c8-7f6658eb0080"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("11a4b47e-65c6-42eb-a1a0-b8e11a1f6c6e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("1a36d5a7-85f5-43ff-a8a6-ea1b5d0b54dc"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2f681d09-3b67-4a6d-bde2-f3f5afef3c5a"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4a0ef2f4-ec8f-48cb-8b88-f68c5e497227"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("58665a65-b5bc-4748-89b5-79e79cafe9bc"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5a6ed8b2-4b2c-4d1e-bf9c-ef58c5c72a44"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6b399ace-1882-4140-b42d-67f205d700d2"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("88d5353b-c64d-46d5-9e66-d68dc4f170c7"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8d0c7b8b-63ab-46f0-bc52-6f2950277e47"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ae3f5e2a-cb26-463c-bcb0-c1f4e094a013"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b62aa26f-b37b-42d5-8bcd-82d9159ac4b0"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b6fbb1f0-c86c-4c09-8c47-8973a536e818"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("bc2e2baf-f5f3-43e4-bb3d-bd2c56374d93"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c88a047b-16b0-4425-8f7d-0f14c55f7b88"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d6f2d00e-e4e3-4a5c-8e3a-9c20a02f65c8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e5c3fda8-9d48-4a39-83c5-47f4d4eb13b1"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ebc45d37-d80c-44e8-8e45-e86575f7c6ae"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ee1b4b55-2a41-4e63-9754-3e1c9d676728"));

            migrationBuilder.DropColumn(
                name: "PricePerPiecePackVAT",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VATCategoryId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Variety",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VATCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VATCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CategoryName", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("6bf2fd3c-1185-47c4-870f-32738d045f36"), "Nejaka burina", new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2263), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2265) },
                    { new Guid("7905728d-ce7d-486b-a981-2882232f1b6b"), "Nejaky strom", new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2274), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2274) }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("412ceb2b-ca6a-43c9-80e1-6eb1cb16164a"), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2353), "Internal", new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2353) },
                    { new Guid("4fd1cbf4-bef4-4fee-b72f-fac1b15c8357"), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2349), "Bennials", new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2350) },
                    { new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2352), "Syngenta", new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2352) },
                    { new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2350), "Schneider", new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2351) },
                    { new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2347), "Volmary", new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2347) }
                });

            migrationBuilder.InsertData(
                table: "VATCategories",
                columns: new[] { "Id", "CreatedAt", "Name", "Rate", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2418), "Reduced", 19m, new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2419) },
                    { new Guid("5bfc3ed5-8874-4452-9043-22065fc00e29"), new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2415), "Normal", 21m, new DateTime(2024, 12, 12, 19, 8, 20, 658, DateTimeKind.Utc).AddTicks(2415) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_PLUCode",
                table: "Products",
                column: "PLUCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_VATCategoryId",
                table: "Products",
                column: "VATCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_VATCategories_VATCategoryId",
                table: "Products",
                column: "VATCategoryId",
                principalTable: "VATCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_VATCategories_VATCategoryId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "VATCategories");

            migrationBuilder.DropIndex(
                name: "IX_Products_PLUCode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SupplierId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_VATCategoryId",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6bf2fd3c-1185-47c4-870f-32738d045f36"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7905728d-ce7d-486b-a981-2882232f1b6b"));

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VATCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Variety",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerPiecePackVAT",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CategoryName", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("0c1d6c90-4937-4b87-b8c8-7f6658eb0080"), "LISTOVÁ ZELENINA", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("11a4b47e-65c6-42eb-a1a0-b8e11a1f6c6e"), "Nástroje a nářadí", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("1a36d5a7-85f5-43ff-a8a6-ea1b5d0b54dc"), "Substráty, hnojiva a ostatní materiály", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("2f681d09-3b67-4a6d-bde2-f3f5afef3c5a"), "BYLINKY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("4a0ef2f4-ec8f-48cb-8b88-f68c5e497227"), "DENIVKY A IRISY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("58665a65-b5bc-4748-89b5-79e79cafe9bc"), "Vazba a aranžmá", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("5a6ed8b2-4b2c-4d1e-bf9c-ef58c5c72a44"), "CHRYZANTÉMY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("6b399ace-1882-4140-b42d-67f205d700d2"), "Cibuloviny", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("88d5353b-c64d-46d5-9e66-d68dc4f170c7"), "POKOJOVÉ A PŘENOSNÉ ROSTLINY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("8d0c7b8b-63ab-46f0-bc52-6f2950277e47"), "Papriky", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("ae3f5e2a-cb26-463c-bcb0-c1f4e094a013"), "Osiva", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("b62aa26f-b37b-42d5-8bcd-82d9159ac4b0"), "Vřes", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("b6fbb1f0-c86c-4c09-8c47-8973a536e818"), "Keře a stromy (Ovocné)", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("bc2e2baf-f5f3-43e4-bb3d-bd2c56374d93"), "TYKVE - CUKETY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("c88a047b-16b0-4425-8f7d-0f14c55f7b88"), "RAJČATA, LILEK", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("d6f2d00e-e4e3-4a5c-8e3a-9c20a02f65c8"), "BALKÓNOVÉ ROSTLINY, LETNIČKY, DVOULETKY, TRVALKY A TRÁVY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("e5c3fda8-9d48-4a39-83c5-47f4d4eb13b1"), "OKURKY ROUBOVANÉ, Pravokořenné", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("ebc45d37-d80c-44e8-8e45-e86575f7c6ae"), "Keře a stromy (Okrasné)", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("ee1b4b55-2a41-4e63-9754-3e1c9d676728"), "Podzimní košík s květinami", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }
    }
}
