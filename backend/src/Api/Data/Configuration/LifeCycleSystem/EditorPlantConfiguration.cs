using Fei.Is.Api.Data.Models.LifeCycleSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.LifeCycleSystem
{
    public class EditorPlantConfiguration : IEntityTypeConfiguration<EditorPlant>
    {
        public void Configure(EntityTypeBuilder<EditorPlant> builder)
        {
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);

            builder.Property(p => p.Type).IsRequired().HasMaxLength(100);

            builder.Property(p => p.Width).IsRequired();

            builder.Property(p => p.Height).IsRequired();

            builder.Property(p => p.PosX).IsRequired();

            builder.Property(p => p.PosY).IsRequired();

            builder.Property(p => p.DateCreated).IsRequired();

            builder.Property(p => p.CurrentDay).IsRequired();

            builder.Property(p => p.Stage).IsRequired().HasMaxLength(100);

            builder.Property(p => p.CurrentState).IsRequired().HasMaxLength(100);

            builder.Property(p => p.EditorBoardId).HasMaxLength(100);

            builder.Ignore(p => p.PlantDetails);

            builder
                .HasOne<EditorBoard>()
                .WithMany(b => b.Plants)
                .HasForeignKey(p => p.EditorBoardId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.HasOne(b => b.GreenHouse).WithMany(g => g.Plants).HasForeignKey(b => b.GreenHouseId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
