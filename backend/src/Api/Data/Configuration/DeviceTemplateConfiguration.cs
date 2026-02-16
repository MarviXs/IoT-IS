using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class DeviceTemplateConfiguration : IEntityTypeConfiguration<DeviceTemplate>
{
    public void Configure(EntityTypeBuilder<DeviceTemplate> builder)
    {
        builder.Property(template => template.IsSyncedFromHub).IsRequired().HasDefaultValue(false);
        builder
            .HasMany(template => template.Sensors)
            .WithOne(sensor => sensor.DeviceTemplate)
            .HasForeignKey(sensor => sensor.DeviceTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(template => template.Recipes)
            .WithOne(recipe => recipe.DeviceTemplate)
            .HasForeignKey(recipe => recipe.DeviceTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(template => template.Recipes)
            .WithOne(recipe => recipe.DeviceTemplate)
            .HasForeignKey(recipe => recipe.DeviceTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(template => template.Controls)
            .WithOne(control => control.DeviceTemplate)
            .HasForeignKey(control => control.DeviceTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(template => template.Devices)
            .WithOne(device => device.DeviceTemplate)
            .HasForeignKey(device => device.DeviceTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(template => template.Name);
    }
}
