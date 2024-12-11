using System.Reflection.Emit;
using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PLUCode).IsRequired();

        builder.HasIndex(p => p.PLUCode).IsUnique();

        builder.Property(p => p.Code).IsRequired();

        builder.Property(p => p.LatinName).IsRequired();
    }
}
