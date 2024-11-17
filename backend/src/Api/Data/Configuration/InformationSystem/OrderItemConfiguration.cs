using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        // Vzťah s objednávkou
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems) // Predpokladáme, že Order má kolekciu OrderItems
            .HasForeignKey(oi => oi.OrderId);

        // Vzťah s produktom
        builder.HasOne(oi => oi.Product)
            .WithMany() // Žiadna navigácia z Product späť na OrderItem
            .OnDelete(DeleteBehavior.Restrict); // Zamedziť mazaniu produktov pri mazaní položiek

        // Vzťah s kontajnerom
        builder.HasOne(oi => oi.Container)
            .WithMany() // Žiadna navigácia z OrderItemContainer späť na OrderItem
            .HasForeignKey(oi => oi.ContainerId)
            .OnDelete(DeleteBehavior.Cascade); // Pri mazaní kontajnera sa zmažú jeho položky

        // Nastavenia polí
        builder.Property(oi => oi.Quantity)
            .IsRequired();
    }
}
