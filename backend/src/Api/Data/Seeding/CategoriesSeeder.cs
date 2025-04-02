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
                    },
                    new Category()
                    {
                        // Pre kvetiny – použité aj ako VAT kategória pre kvetiny
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Category()
                    {
                        // Pre produkty, kde kategória reprezentuje napr. ruže
                        Id = new Guid("412ceb2b-ca6a-43c9-80e1-6eb1cb16164a"),
                        CategoryName = "Růže",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Category()
                    {
                        // Ďalšia kategória kvetín – napr. exotické kvetiny
                        Id = new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"),
                        CategoryName = "Exotické kvetiny",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Category()
                    {
                        // Pre prácu a produktom ako hlina
                        Id = new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"),
                        CategoryName = "Práca",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    }
            ]);
        }
    }
}
