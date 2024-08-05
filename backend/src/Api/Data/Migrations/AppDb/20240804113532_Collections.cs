using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class Collections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceCollections_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubCollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionItems_DeviceCollections_CollectionParentId",
                        column: x => x.CollectionParentId,
                        principalTable: "DeviceCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionItems_DeviceCollections_SubCollectionId",
                        column: x => x.SubCollectionId,
                        principalTable: "DeviceCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CollectionItems_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_AccessToken",
                table: "Devices",
                column: "AccessToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_CollectionParentId",
                table: "CollectionItems",
                column: "CollectionParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_DeviceId",
                table: "CollectionItems",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_SubCollectionId",
                table: "CollectionItems",
                column: "SubCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCollections_OwnerId",
                table: "DeviceCollections",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionItems");

            migrationBuilder.DropTable(
                name: "DeviceCollections");

            migrationBuilder.DropIndex(
                name: "IX_Devices_AccessToken",
                table: "Devices");
        }
    }
}
