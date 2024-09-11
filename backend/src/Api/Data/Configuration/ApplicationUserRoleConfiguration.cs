using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fei.Is.Api.Data.Configuration;

public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.HasData(
            new ApplicationUserRole
            {
                RoleId = new Guid("00000000-0000-0000-0000-000000000001"),
                UserId = new Guid("00000000-0000-0000-0000-000000000001")
            }
        );
    }
}
