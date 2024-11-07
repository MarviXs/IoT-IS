using Fei.Is.Api.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class SceneSensorTriggerConfiguration : IEntityTypeConfiguration<SceneSensorTrigger>
{
    public void Configure(EntityTypeBuilder<SceneSensorTrigger> builder)
    {
        builder
            .HasIndex(s => new
            {
                s.SceneId,
                s.DeviceId,
                s.SensorTag
            })
            .IsUnique();
    }
}
