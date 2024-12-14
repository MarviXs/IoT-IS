using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class VATCategoryConfiguration : IEntityTypeConfiguration<VATCategory>
{
    public void Configure(EntityTypeBuilder<VATCategory> builder)
    {
        builder.HasKey(category => category.Id);
    }
}
