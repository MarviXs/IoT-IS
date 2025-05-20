using Fei.Is.Api.Data.Models.LifeCycleSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.LifeCycleSystem
{
    public class GreenHouseConfiguration : IEntityTypeConfiguration<GreenHouse>
    {
        public void Configure(EntityTypeBuilder<GreenHouse> builder)
        {
            builder.Property(g => g.Name).IsRequired().HasMaxLength(100);

            builder.Property(g => g.Width).IsRequired();
            builder.Property(g => g.Depth).IsRequired();
            builder.Property(g => g.DateCreated).IsRequired();

            builder.HasMany(g => g.Pots).WithOne(b => b.GreenHouse).HasForeignKey(b => b.GreenHouseId).OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(g => g.Plants).WithOne(p => p.GreenHouse).HasForeignKey(p => p.GreenHouseId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
