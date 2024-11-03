using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class AdditionalOrderConfiguration : IEntityTypeConfiguration<AdditionalOrder>
{
    public void Configure(EntityTypeBuilder<AdditionalOrder> builder)
    {
        builder.HasKey(ao => ao.Id);

        builder.HasOne(ao => ao.Product)
            .WithMany()
            .HasForeignKey(ao => ao.ProductId);
    }
}
