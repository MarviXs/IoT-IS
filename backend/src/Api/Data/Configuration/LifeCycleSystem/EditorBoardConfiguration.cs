using Fei.Is.Api.Data.Models.LifeCycleSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.LifeCycleSystem
{
    public class EditorBoardConfiguration : IEntityTypeConfiguration<EditorBoard>
    {
        public void Configure(EntityTypeBuilder<EditorBoard> builder)
        {
            builder.Property(b => b.Name).IsRequired().HasMaxLength(100);

            builder.Property(b => b.Columns).IsRequired();

            builder.Property(b => b.Rows).IsRequired();

            builder.Property(b => b.Width).IsRequired();

            builder.Property(b => b.Height).IsRequired();

            builder.Property(b => b.PosX).IsRequired();

            builder.Property(b => b.PosY).IsRequired();

            builder.Property(b => b.Shape).IsRequired().HasMaxLength(100);

            builder.Property(b => b.DateCreated).IsRequired();

            builder.Ignore(b => b.Label);

            builder.HasOne(b => b.GreenHouse).WithMany(g => g.Pots).HasForeignKey(b => b.GreenHouseId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
