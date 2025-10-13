using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class JobScheduleConfiguration : IEntityTypeConfiguration<JobSchedule>
{
    public void Configure(EntityTypeBuilder<JobSchedule> builder)
    {
        builder
            .HasOne(schedule => schedule.Device)
            .WithMany(device => device.JobSchedules)
            .HasForeignKey(schedule => schedule.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(schedule => schedule.WeekDays)
            .WithOne(day => day.JobSchedule)
            .HasForeignKey(day => day.JobScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
