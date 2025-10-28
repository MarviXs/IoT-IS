using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class DeviceFirmwareConfiguration : IEntityTypeConfiguration<DeviceFirmware>
{
    public void Configure(EntityTypeBuilder<DeviceFirmware> builder)
    {
        builder
            .HasOne(firmware => firmware.DeviceTemplate)
            .WithMany(template => template.Firmwares)
            .HasForeignKey(firmware => firmware.DeviceTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(firmware => firmware.VersionNumber).IsRequired().HasMaxLength(128);
        builder.Property(firmware => firmware.OriginalFileName).IsRequired();
        builder.Property(firmware => firmware.StoredFileName).IsRequired();

        builder.HasIndex(firmware => new { firmware.DeviceTemplateId, firmware.VersionNumber }).IsUnique();
    }
}
