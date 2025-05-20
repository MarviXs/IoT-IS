using Fei.Is.Api.Data.Models.LifeCycleSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.LifeCycleSystem;

public class PlantAnalysisConfiguration : IEntityTypeConfiguration<PlantAnalysis>
{
    public void Configure(EntityTypeBuilder<PlantAnalysis> builder)
    {
        builder.Property(a => a.AnalysisDate).IsRequired();

        builder.Property(a => a.Height).IsRequired();

        builder.Property(a => a.Width).IsRequired();

        builder.Property(a => a.LeafCount).IsRequired();

        builder.Property(a => a.Area).IsRequired();

        builder.Property(a => a.ImageName).IsRequired();

        builder.Property(a => a.Disease).HasMaxLength(200);

        builder.Property(a => a.Health).HasMaxLength(200);

        builder.HasOne(a => a.Plant).WithMany(p => p.Analyses).HasForeignKey(a => a.PlantId).OnDelete(DeleteBehavior.Cascade);
    }
}
