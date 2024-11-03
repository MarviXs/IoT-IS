using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired();

        builder.HasMany(c => c.DeliveryNotes)
            .WithOne(dn => dn.Supplier);

        builder.HasMany(c => c.Invoices)
            .WithOne(i => i.Supplier);
    }
}
