using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class OrderItemContainerConfiguration : IEntityTypeConfiguration<OrderItemContainer>
{
    public void Configure(EntityTypeBuilder<OrderItemContainer> builder)
    {
        builder.HasKey(oic => oic.Id);

        builder.Property(oic => oic.Name)
            .IsRequired()
            .HasMaxLength(255); // Maximálna dĺžka názvu

        builder.Property(oic => oic.PricePerContainer)
            .HasPrecision(18, 2); // Presnosť pre desatinné hodnoty

        builder.Property(oic => oic.TotalPrice)
            .HasPrecision(18, 2); // Presnosť pre celkovú cenu

        // Nastavenie vzťahu s produktmi
        builder.HasMany(oic => oic.Products)
            .WithOne() // Žiadna navigácia späť z Product na OrderItemContainer
            .OnDelete(DeleteBehavior.Cascade); // Pri mazaní kontajnera sa zmažú jeho produkty
    }
}
