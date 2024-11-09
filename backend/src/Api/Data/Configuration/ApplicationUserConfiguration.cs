using Fei.Is.Api.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasMany(e => e.UserRoles).WithOne(e => e.User).HasForeignKey(ur => ur.UserId).IsRequired();

        builder
            .HasMany(e => e.DeviceSharesTo)
            .WithOne(e => e.SharedToUser)
            .HasForeignKey(e => e.SharedToUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
        builder
            .HasMany(e => e.DeviceSharesFrom)
            .WithOne(e => e.SharingUser)
            .HasForeignKey(e => e.SharingUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
