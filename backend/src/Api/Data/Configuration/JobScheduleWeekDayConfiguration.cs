using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class JobScheduleWeekDayConfiguration : IEntityTypeConfiguration<JobScheduleWeekDay>
{
    public void Configure(EntityTypeBuilder<JobScheduleWeekDay> builder)
    {
        builder.HasIndex(day => new { day.JobScheduleId, day.Day }).IsUnique();
    }
}
