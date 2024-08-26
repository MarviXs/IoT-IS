using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class DeviceCollectionConfiguration : IEntityTypeConfiguration<DeviceCollection>
{
    public void Configure(EntityTypeBuilder<DeviceCollection> builder)
    {
        builder
            .HasMany(collection => collection.ParentItems)
            .WithOne(item => item.CollectionParent)
            .HasForeignKey(item => item.CollectionParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(collection => collection.ChildItems)
            .WithOne(item => item.SubCollection)
            .HasForeignKey(item => item.SubCollectionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(collection => collection.RootCollection)
            .WithMany()
            .HasForeignKey(collection => collection.RootCollectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(collection => collection.AllCollectionsFlat).WithOne().HasForeignKey(item => item.Id).OnDelete(DeleteBehavior.Cascade);
    }
}
