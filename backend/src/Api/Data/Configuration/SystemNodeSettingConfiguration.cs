using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class SystemNodeSettingConfiguration : IEntityTypeConfiguration<SystemNodeSetting>
{
    public void Configure(EntityTypeBuilder<SystemNodeSetting> builder)
    {
        builder.Property(setting => setting.NodeType).IsRequired().HasDefaultValue(SystemNodeType.Hub);
        builder.Property(setting => setting.HubUrl).HasMaxLength(1024);
        builder.Property(setting => setting.HubToken).HasMaxLength(256);
    }
}
