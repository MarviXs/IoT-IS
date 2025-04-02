using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations.TimescaleDb
{
    /// <inheritdoc />
    public partial class Hypercore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"DataPoints\" SET (timescaledb.enable_columnstore, timescaledb.segmentby = '\"DeviceId\", \"SensorTag\"', timescaledb.orderby = '\"TimeStamp\" DESC')"
            );
            migrationBuilder.Sql("CALL add_columnstore_policy('\"DataPoints\"', INTERVAL '7d');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql("CALL remove_columnstore_policy('\"DataPoints\"');");
            migrationBuilder.Sql("ALTER TABLE \"DataPoints\" RESET (timescaledb.enable_columnstore, timescaledb.segmentby, timescaledb.orderby)");
        }
    }
}
