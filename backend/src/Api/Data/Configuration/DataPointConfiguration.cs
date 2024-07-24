using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class DataPointConfiguration : IEntityTypeConfiguration<DataPoint>
{
    public void Configure(EntityTypeBuilder<DataPoint> builder)
    {
        builder.HasNoKey();
        builder
            .HasIndex(
                d =>
                    new
                    {
                        d.DeviceId,
                        d.SensorTag,
                        d.TimeStamp
                    }
            )
            .IsDescending(false, false, true);
    }
}
