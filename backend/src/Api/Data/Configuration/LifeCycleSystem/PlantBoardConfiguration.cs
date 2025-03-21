using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fei.Is.Api.Data.Models.LifeCycleSystem;

namespace Fei.Is.Api.Data.Configuration.LifeCycleSystem
{
    public class PlantBoardConfiguration : IEntityTypeConfiguration<PlantBoard>
    {
        public void Configure(EntityTypeBuilder<PlantBoard> builder)
        {
            builder.HasKey(p => p.PlantBoardId);

            builder.Property(p => p.PlantBoardId)
                .HasMaxLength(100);

            builder.Property(p => p.Rows)
                .IsRequired();

            builder.Property(p => p.Cols)
                .IsRequired();

            builder.HasMany(p => p.Plants)
                .WithOne()
                .HasForeignKey(pl => pl.PlantBoardId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}