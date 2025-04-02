using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Fei.Is.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        if (args.Contains("seed"))
        {
            SeedDatabase(host);
            return;
        }

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    public static void SeedDatabase(IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var seedTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ISeed).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

            List<ISeed> seeders = new();

            foreach (var seedType in seedTypes)
            {
                var seederInstance =
                    (ISeed)(Activator.CreateInstance(seedType) ?? throw new InvalidOperationException($"Could not create an instance of {seedType.Name}"));
                seeders.Add(seederInstance);
            }

            List<Type> seededTypes = new();

            while (seeders.Count > 0)
            {
                List<ISeed> toRemove = new();

                foreach (var seeder in seeders)
                {
                    if (seeder.GetDependencies().All(d => seededTypes.Contains(d)))
                    {
                        seeder.Seed(dbContext);
                        seededTypes.Add(seeder.GetModel());
                        toRemove.Add(seeder);
                    }
                }

                foreach (var seeder in toRemove)
                {
                    seeders.Remove(seeder);
                }
            }
        }
    }
}
