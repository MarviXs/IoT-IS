using Fei.Is.Api.Data.Configuration;
using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Contexts;

public class TimeScaleDbContext : DbContext
{
    public TimeScaleDbContext(DbContextOptions<TimeScaleDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new DataPointConfiguration());

        modelBuilder.HasDbFunction(() => TimeBucket(default!, default!));
        modelBuilder.HasDbFunction(() => Lttb(default!, default!, default!));
    }

    [DbFunction("time_bucket", Schema = "public")]
    public static DateTimeOffset TimeBucket(TimeSpan bucket_width, DateTimeOffset ts) => throw new NotImplementedException();

    [DbFunction("lttb", Schema = "public")]
    public static DateTimeOffset Lttb(DateTimeOffset ts, double value, int resolution) => throw new NotImplementedException();

    public DbSet<DataPoint> DataPoints { get; set; }
}
