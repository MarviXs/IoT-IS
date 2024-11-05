using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class IS_Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("1804fc51-a765-417c-b390-9ffb957b75f6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2d43f4b3-c5f4-4097-a471-30996b0c9894"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3525b3ad-e90f-4fd5-a588-5cb3dc5e2024"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3f43c95b-4a35-42ba-b542-be61e836a1b8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3fcf4483-2d74-49da-8c13-83fbaaaf2abe"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44265d72-34a0-40fe-bc37-256aaac2120f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("56aefbe5-813d-4089-a291-382bc7c0bc9f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5d96313a-46b8-4998-bae6-98e2527fdbdc"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7ef51fe1-4f14-4d36-a53e-aa327de57afe"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7f0ad469-b458-4f45-9eb2-8b4a02621b1e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("812a5ae3-47a7-4122-83ee-9cdc446dc1b6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a25fdc74-49be-4687-8bc5-c2249af04258"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("be0f2f69-9fa7-4cce-a71a-16180d140a51"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("beb811f3-3e50-46d2-a25c-d48e7c2438ee"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("cfac1f57-04b7-45b7-934d-361a46ffe85b"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d721b2f0-4dad-4e4f-8f32-9d280ee90212"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("dfeaad02-9283-4617-83d0-c93e2dd5e569"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ea37a145-665b-47af-b600-30fdac0a22c3"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("fb35fdce-8f70-400a-b0ae-df0d83156181"));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CategoryName", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1804fc51-a765-417c-b390-9ffb957b75f6"), "Podzimní košík s květinami", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5047), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5048) },
                    { new Guid("2d43f4b3-c5f4-4097-a471-30996b0c9894"), "Osiva", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5054), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5054) },
                    { new Guid("3525b3ad-e90f-4fd5-a588-5cb3dc5e2024"), "Keře a stromy (Okrasné)", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5056), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5056) },
                    { new Guid("3f43c95b-4a35-42ba-b542-be61e836a1b8"), "DENIVKY A IRISY", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5057), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5058) },
                    { new Guid("3fcf4483-2d74-49da-8c13-83fbaaaf2abe"), "POKOJOVÉ A PŘENOSNÉ ROSTLINY", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5032), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5032) },
                    { new Guid("44265d72-34a0-40fe-bc37-256aaac2120f"), "BYLINKY", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5042), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5042) },
                    { new Guid("56aefbe5-813d-4089-a291-382bc7c0bc9f"), "Nástroje a nářadí", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5064), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5064) },
                    { new Guid("5d96313a-46b8-4998-bae6-98e2527fdbdc"), "LISTOVÁ ZELENINA", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5040), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5041) },
                    { new Guid("7ef51fe1-4f14-4d36-a53e-aa327de57afe"), "Cibuloviny", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5051), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5051) },
                    { new Guid("7f0ad469-b458-4f45-9eb2-8b4a02621b1e"), "BALKÓNOVÉ ROSTLINY, LETNIČKY, DVOULETKY, TRVALKY A TRÁVY", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(4942), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(4945) },
                    { new Guid("812a5ae3-47a7-4122-83ee-9cdc446dc1b6"), "OKURKY ROUBOVANÉ, Pravokořenné", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5034), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5034) },
                    { new Guid("a25fdc74-49be-4687-8bc5-c2249af04258"), "Vazba a aranžmá", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5059), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5059) },
                    { new Guid("be0f2f69-9fa7-4cce-a71a-16180d140a51"), "Keře a stromy (Ovocné)", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5062), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5063) },
                    { new Guid("beb811f3-3e50-46d2-a25c-d48e7c2438ee"), "Substráty, hnojiva a ostatní materiály", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5052), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5053) },
                    { new Guid("cfac1f57-04b7-45b7-934d-361a46ffe85b"), "CHRYZANTÉMY", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5044), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5044) },
                    { new Guid("d721b2f0-4dad-4e4f-8f32-9d280ee90212"), "RAJČATA, LILEK", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5037), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5038) },
                    { new Guid("dfeaad02-9283-4617-83d0-c93e2dd5e569"), "Vřes", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5049), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5049) },
                    { new Guid("ea37a145-665b-47af-b600-30fdac0a22c3"), "TYKVE - CUKETY", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5036), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5036) },
                    { new Guid("fb35fdce-8f70-400a-b0ae-df0d83156181"), "Papriky", new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5039), new DateTime(2024, 11, 4, 19, 4, 25, 375, DateTimeKind.Utc).AddTicks(5039) }
                });
        }
    }
}
