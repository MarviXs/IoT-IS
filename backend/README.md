dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Data/Migrations/AppDb
dotnet ef database update --context AppDbContext

dotnet ef migrations add InitialCreate --context TimeScaleDbContext --output-dir Data/Migrations/TimescaleDb
dotnet ef database update --context TimeScaleDbContext

## Timescale DB migrations
migrationBuilder.Sql("SELECT create_hypertable('\"DataPoints\"', by_range('TimeStamp'))");
migrationBuilder.Sql(
    "ALTER TABLE \"DataPoints\" SET (timescaledb.compress, timescaledb.compress_segmentby = '\"DeviceId\", \"SensorTag\"', timescaledb.compress_orderby = '\"TimeStamp\"')"
);
