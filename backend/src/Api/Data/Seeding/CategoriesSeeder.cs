using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace Fei.Is.Api.Data.Seeding
{
    public class CategoriesSeeder : ISeed
    {
        public List<Type> GetDependencies()
        {
            return new List<Type>();
        }

        public Type GetModel()
        {
            return typeof(Category);
        }

        public void Seed(AppDbContext appDbContext)
        {
            appDbContext.Categories.AddRange(
                [
                    new Category()
                    {
                        Id = Guid.NewGuid(),
                        CategoryName = "Nejaka burina",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Category()
                    {
                        Id = Guid.NewGuid(),
                        CategoryName = "Nejaky strom",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    }
                ]
            );
            appDbContext.SaveChanges();
        }
    }
}
