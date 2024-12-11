using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public class CategoriesSeeder : ISeed
    {
        public void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Category>()
                .HasData(
                    new Category()
                    {
                        Id = new Guid("6bf2fd3c-1185-47c4-870f-32738d045f36"),
                        CategoryName = "Nejaka burina"
                    },
                    new Category()
                    {
                        Id = new Guid("7905728d-ce7d-486b-a981-2882232f1b6b"),
                        CategoryName = "Nejaky strom"
                    }
                );
        }
    }
}
