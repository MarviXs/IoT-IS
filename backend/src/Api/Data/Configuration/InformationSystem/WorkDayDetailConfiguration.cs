using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem;

public class WorkDayDetailConfiguration : IEntityTypeConfiguration<WorkDayDetail>
{
    public void Configure(EntityTypeBuilder<WorkDayDetail> builder)
    {
        builder.HasKey(wd => wd.Id);

        builder.HasOne(wd => wd.WorkReport)
            .WithMany(wr => wr.WorkDayDetails)
            .HasForeignKey(wd => wd.Id);
    }
}
