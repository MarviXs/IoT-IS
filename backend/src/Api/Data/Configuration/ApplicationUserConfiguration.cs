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

        builder.HasData(
            new ApplicationUser
            {
                Id = new Guid("00000000-0000-0000-0000-000000000001"),
                UserName = "admin@example.com",
                Email = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null, "Admin@123"),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        );
    }
}
