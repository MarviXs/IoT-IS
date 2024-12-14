using Fei.Is.Api.Data.Models.InformationSystem;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public class SuppliersSeeder : ISeed
    {
        public void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Supplier>()
                .HasData(
                    new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Volmary",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Supplier()
                    {
                        Id = new Guid("4fd1cbf4-bef4-4fee-b72f-fac1b15c8357"),
                        Name = "Bennials",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Supplier()
                    {
                        Id = new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"),
                        Name = "Schneider",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Supplier()
                    {
                        Id = new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"),
                        Name = "Syngenta",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new Supplier()
                    {
                        Id = new Guid("412ceb2b-ca6a-43c9-80e1-6eb1cb16164a"),
                        Name = "Internal",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    }
                );
        }
    }
}
