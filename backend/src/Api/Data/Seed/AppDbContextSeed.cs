using Fei.Is.Api.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Data.Seed;

public class AppDbContextSeed
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        await SeedDefaultUserAsync(userManager, roleManager);
    }

    public static async Task SeedDefaultUserAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var user = new ApplicationUser()
        {
            Id = Guid.NewGuid(),
            UserName = "admin@example.com",
            Email = "admin@example.com",
            NormalizedUserName = "ADMIN@EXAMPLE.COM",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var existingUser = await userManager.FindByEmailAsync(user.Email);
        if (existingUser == null)
        {
            await userManager.CreateAsync(user, "Admin@123");
        }

        var role = new ApplicationRole()
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            NormalizedName = "ADMIN"
        };

        var existingRole = await roleManager.FindByNameAsync(role.Name);
        if (existingRole == null)
        {
            await roleManager.CreateAsync(role);
        }

        await userManager.AddToRoleAsync(user, role.Name);
    }
}
