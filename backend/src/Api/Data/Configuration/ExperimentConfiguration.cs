using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class ExperimentConfiguration : IEntityTypeConfiguration<Experiment>
{
    public void Configure(EntityTypeBuilder<Experiment> builder)
    {
        builder.HasIndex(e => e.OwnerId);

        builder.HasOne(e => e.Owner).WithMany().HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.RecipeToRun).WithMany().HasForeignKey(e => e.RecipeToRunId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(e => e.RanJob).WithMany().HasForeignKey(e => e.RanJobId).OnDelete(DeleteBehavior.NoAction);
    }
}
