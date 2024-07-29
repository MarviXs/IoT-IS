using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasMany(j => j.Commands).WithOne(c => c.Job).HasForeignKey(rs => rs.JobId).OnDelete(DeleteBehavior.Cascade);
    }
}
