using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.TimescaleDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataPoints",
                columns: table =>
                    new
                    {
                        DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                        SensorTag = table.Column<string>(type: "text", nullable: false),
                        TimeStamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                        Value = table.Column<double>(type: "double precision", nullable: true)
                    },
                constraints: table => { }
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataPoints_DeviceId_SensorTag_TimeStamp",
                table: "DataPoints",
                columns: new[] { "DeviceId", "SensorTag", "TimeStamp" },
                descending: new[] { false, false, true }
            );

            migrationBuilder.Sql("SELECT create_hypertable('\"DataPoints\"', by_range('TimeStamp'))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DataPoints");
        }
    }
}
