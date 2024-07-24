using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class JobStatusConfiguration : IEntityTypeConfiguration<JobStatus>
{
    public void Configure(EntityTypeBuilder<JobStatus> builder)
    {
        builder.HasStorageParameter("fillfactor", 70);
    }
}
