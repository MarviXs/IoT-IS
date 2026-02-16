using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasIndex(device => device.AccessToken).IsUnique();
        builder.Property(device => device.IsSyncedFromHub).IsRequired().HasDefaultValue(false);
        builder
            .HasMany(device => device.SharedWithUsers)
            .WithOne(share => share.Device)
            .HasForeignKey(share => share.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
