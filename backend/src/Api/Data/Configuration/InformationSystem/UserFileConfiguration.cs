using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.InformationSystem
{
    public class UserFileConfiguration : IEntityTypeConfiguration<UserFile>
    {
        public void Configure(EntityTypeBuilder<UserFile> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.FileIdentifier)
                .HasConversion(
                    v => v.ToString(),
                    v => (FileIdentifier)Enum.Parse(typeof(FileIdentifier), v));
        }
    }
}
