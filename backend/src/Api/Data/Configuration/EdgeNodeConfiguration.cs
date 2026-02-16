using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class EdgeNodeConfiguration : IEntityTypeConfiguration<EdgeNode>
{
    public void Configure(EntityTypeBuilder<EdgeNode> builder)
    {
        builder.Property(edgeNode => edgeNode.Name).IsRequired().HasMaxLength(128);
        builder.Property(edgeNode => edgeNode.Token).IsRequired().HasMaxLength(256);

        builder.HasIndex(edgeNode => edgeNode.Token).IsUnique();
    }
}
