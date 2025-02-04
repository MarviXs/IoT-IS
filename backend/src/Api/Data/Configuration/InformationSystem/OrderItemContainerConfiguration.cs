using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class OrderItemContainerConfiguration : IEntityTypeConfiguration<OrderItemContainer>
{
    public void Configure(EntityTypeBuilder<OrderItemContainer> builder)
    {
        // Primárny kľúč
        builder.HasKey(oic => oic.Id);

        builder.Property(oic => oic.PricePerContainer).HasPrecision(18, 2);

        builder.Property(oic => oic.TotalPrice).HasPrecision(18, 2);
        builder.HasMany(o => o.Items).WithOne();
    }
}
