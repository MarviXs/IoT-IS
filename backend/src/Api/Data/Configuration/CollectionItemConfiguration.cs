using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class CollectionItemConfiguration : IEntityTypeConfiguration<CollectionItem>
{
    public void Configure(EntityTypeBuilder<CollectionItem> builder)
    {
        builder
            .HasOne(collectionItem => collectionItem.CollectionParent)
            .WithMany(deviceCollection => deviceCollection.ChildItems)
            .HasForeignKey(collectionItem => collectionItem.CollectionParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(collectionItem => collectionItem.Device)
            .WithMany(device => device.CollectionItems)
            .HasForeignKey(collectionItem => collectionItem.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(collectionItem => collectionItem.SubCollection)
            .WithMany(deviceCollection => deviceCollection.ParentItems)
            .HasForeignKey(collectionItem => collectionItem.SubCollectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
