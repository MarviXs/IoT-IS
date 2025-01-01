using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fei.Is.Api.Data.Models.LifeCycleSystem;

namespace Fei.Is.Api.Data.Configuration.LifeCycleSystem;

public class PlantAnalysisConfiguration : IEntityTypeConfiguration<PlantAnalysis>
{
    public void Configure(EntityTypeBuilder<PlantAnalysis> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AnalysisDate)
            .IsRequired();

        builder.Property(a => a.Height)
            .IsRequired();

        builder.Property(a => a.Width)
            .IsRequired();

        builder.Property(a => a.LeafCount)
            .IsRequired();

        builder.Property(a => a.Area)
            .IsRequired();

        builder.Property(a => a.Disease)
            .HasMaxLength(200);

        builder.Property(a => a.Health)
            .HasMaxLength(200);

        builder.HasOne(a => a.Plant)
            .WithMany(p => p.Analyses)
            .HasForeignKey(a => a.PlantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
