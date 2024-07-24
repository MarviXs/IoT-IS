using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasMany(r => r.Steps).WithOne(rs => rs.Recipe).HasForeignKey(rs => rs.RecipeId).OnDelete(DeleteBehavior.Cascade);
    }
}
