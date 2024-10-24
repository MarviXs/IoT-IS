using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class WorkReportConfiguration : IEntityTypeConfiguration<WorkReport>
{
    public void Configure(EntityTypeBuilder<WorkReport> builder)
    {
        builder.HasKey(wr => wr.Id);

        builder.HasOne(wr => wr.Supplier)
            .WithMany()
            .HasForeignKey(wr => wr.SupplierId);

        builder.HasOne(wr => wr.Customer)
            .WithMany()
            .HasForeignKey(wr => wr.CustomerId);
    }
}
