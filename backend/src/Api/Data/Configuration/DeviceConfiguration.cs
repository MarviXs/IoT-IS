using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasIndex(device => device.AccessToken).IsUnique();
        builder.Property(device => device.SyncedFromEdge).IsRequired().HasDefaultValue(false);
        builder
            .HasOne(device => device.SyncedFromEdgeNode)
            .WithMany(edgeNode => edgeNode.SyncedDevices)
            .HasForeignKey(device => device.SyncedFromEdgeNodeId)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasMany(device => device.SharedWithUsers)
            .WithOne(share => share.Device)
            .HasForeignKey(share => share.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
