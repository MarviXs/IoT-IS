using Fei.Is.Api.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder
            .HasOne(rs => rs.Subrecipe)
            .WithMany(sr => sr.ParentSteps)
            .HasForeignKey(rs => rs.SubrecipeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(rs => rs.Command)
            .WithMany(c => c.RecipeSteps)
            .HasForeignKey(rs => rs.CommandId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
