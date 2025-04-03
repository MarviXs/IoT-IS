using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public class VATCategoriesSeeder : ISeed
    {
        public List<Type> GetDependencies()
        {
            return new List<Type>();
        }

        public Type GetModel()
        {
            return typeof(VATCategory);
        }

        public void Seed(AppDbContext appDbContext)
        {
            appDbContext.AddRange([
                new VATCategory()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Normal",
                        Rate = 21,
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new VATCategory()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Reduced",
                        Rate = 19,
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    }
                ]);
        }
    }
}
