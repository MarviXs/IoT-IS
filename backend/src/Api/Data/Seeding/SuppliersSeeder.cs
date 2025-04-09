using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public class SuppliersSeeder : ISeed
    {
        public List<Type> GetDependencies()
        {
            return new List<Type>();
        }

        public Type GetModel()
        {
            return typeof(Supplier);
        }

        public void Seed(AppDbContext appDbContext)
        {
            appDbContext.Suppliers.AddRange(
                [
                    new Supplier()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Volmary",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Supplier()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Bennials",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Supplier()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Schneider",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Supplier()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Syngenta",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Supplier()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Internal",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    }
                ]
            );
        }
    }
}
