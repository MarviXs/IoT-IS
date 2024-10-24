using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class DeliveryItemConfiguration : IEntityTypeConfiguration<DeliveryItem>
{
    public void Configure(EntityTypeBuilder<DeliveryItem> builder)
    {
        builder.HasKey(di => di.Id);

        builder.HasOne(di => di.DeliveryNote)
            .WithMany(dn => dn.DeliveryItems)
            .HasForeignKey(di => di.DeliveryNoteId);
    }
}
