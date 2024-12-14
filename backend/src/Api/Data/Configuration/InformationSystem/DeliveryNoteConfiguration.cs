using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class DeliveryNoteConfiguration : IEntityTypeConfiguration<DeliveryNote>
{
    public void Configure(EntityTypeBuilder<DeliveryNote> builder)
    {
        builder.HasKey(dn => dn.Id);

        builder.Property(dn => dn.DeliveryNumber).IsRequired();
    }
}
