using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class DeviceShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceShares_AspNetUsers_SharedToUserId",
                        column: x => x.SharedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceShares_AspNetUsers_SharingUserId",
                        column: x => x.SharingUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceShares_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceShares_DeviceId",
                table: "DeviceShares",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceShares_SharedToUserId",
                table: "DeviceShares",
                column: "SharedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceShares_SharingUserId",
                table: "DeviceShares",
                column: "SharingUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceShares");
        }
    }
}
