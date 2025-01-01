using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fei.Is.Api.Data.Models.LifeCycleSystem;

namespace Fei.Is.Api.Data.Configuration.LifeCycleSystem;

public class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.DatePlanted)
            .IsRequired();

        builder.HasMany(p => p.Analyses)
            .WithOne(a => a.Plant)
            .HasForeignKey(a => a.PlantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}