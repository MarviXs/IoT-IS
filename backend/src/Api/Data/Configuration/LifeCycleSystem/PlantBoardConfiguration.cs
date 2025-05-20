using Fei.Is.Api.Data.Models.LifeCycleSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration.LifeCycleSystem
{
    public class PlantBoardConfiguration : IEntityTypeConfiguration<PlantBoard>
    {
        public void Configure(EntityTypeBuilder<PlantBoard> builder)
        {
            builder.Property(p => p.Rows).IsRequired();

            builder.Property(p => p.Cols).IsRequired();
        }
    }
}
