using System;
using System.Linq;
using System.Reflection;
using Fei.Is.Api.Data.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyDataSeeds(this ModelBuilder modelBuilder)
        {
            var seedTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ISeed).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var seedType in seedTypes)
            {
                var seederInstance =
                    Activator.CreateInstance(seedType) ?? throw new InvalidOperationException($"Could not create an instance of {seedType.Name}");
                ((ISeed)seederInstance).Seed(modelBuilder);
            }
        }
    }
}
