using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class RootCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RootCollectionId",
                table: "DeviceCollections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceCollectionId",
                table: "CollectionItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCollections_RootCollectionId",
                table: "DeviceCollections",
                column: "RootCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_DeviceCollectionId",
                table: "CollectionItems",
                column: "DeviceCollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionItems_DeviceCollections_DeviceCollectionId",
                table: "CollectionItems",
                column: "DeviceCollectionId",
                principalTable: "DeviceCollections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceCollections_DeviceCollections_RootCollectionId",
                table: "DeviceCollections",
                column: "RootCollectionId",
                principalTable: "DeviceCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionItems_DeviceCollections_DeviceCollectionId",
                table: "CollectionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceCollections_DeviceCollections_RootCollectionId",
                table: "DeviceCollections");

            migrationBuilder.DropIndex(
                name: "IX_DeviceCollections_RootCollectionId",
                table: "DeviceCollections");

            migrationBuilder.DropIndex(
                name: "IX_CollectionItems_DeviceCollectionId",
                table: "CollectionItems");

            migrationBuilder.DropColumn(
                name: "RootCollectionId",
                table: "DeviceCollections");

            migrationBuilder.DropColumn(
                name: "DeviceCollectionId",
                table: "CollectionItems");
        }
    }
}
