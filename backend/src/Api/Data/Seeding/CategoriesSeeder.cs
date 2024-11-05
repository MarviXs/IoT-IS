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
                    new Category { Id = Guid.Parse("d6f2d00e-e4e3-4a5c-8e3a-9c20a02f65c8"), CategoryName = "BALKÓNOVÉ ROSTLINY, LETNIČKY, DVOULETKY, TRVALKY A TRÁVY", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("88d5353b-c64d-46d5-9e66-d68dc4f170c7"), CategoryName = "POKOJOVÉ A PŘENOSNÉ ROSTLINY", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("e5c3fda8-9d48-4a39-83c5-47f4d4eb13b1"), CategoryName = "OKURKY ROUBOVANÉ, Pravokořenné", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("bc2e2baf-f5f3-43e4-bb3d-bd2c56374d93"), CategoryName = "TYKVE - CUKETY", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("c88a047b-16b0-4425-8f7d-0f14c55f7b88"), CategoryName = "RAJČATA, LILEK", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("8d0c7b8b-63ab-46f0-bc52-6f2950277e47"), CategoryName = "Papriky", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("0c1d6c90-4937-4b87-b8c8-7f6658eb0080"), CategoryName = "LISTOVÁ ZELENINA", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("2f681d09-3b67-4a6d-bde2-f3f5afef3c5a"), CategoryName = "BYLINKY", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("5a6ed8b2-4b2c-4d1e-bf9c-ef58c5c72a44"), CategoryName = "CHRYZANTÉMY", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("ee1b4b55-2a41-4e63-9754-3e1c9d676728"), CategoryName = "Podzimní košík s květinami", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("b62aa26f-b37b-42d5-8bcd-82d9159ac4b0"), CategoryName = "Vřes", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("6b399ace-1882-4140-b42d-67f205d700d2"), CategoryName = "Cibuloviny", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("1a36d5a7-85f5-43ff-a8a6-ea1b5d0b54dc"), CategoryName = "Substráty, hnojiva a ostatní materiály", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("ae3f5e2a-cb26-463c-bcb0-c1f4e094a013"), CategoryName = "Osiva", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("ebc45d37-d80c-44e8-8e45-e86575f7c6ae"), CategoryName = "Keře a stromy (Okrasné)", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("4a0ef2f4-ec8f-48cb-8b88-f68c5e497227"), CategoryName = "DENIVKY A IRISY", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("58665a65-b5bc-4748-89b5-79e79cafe9bc"), CategoryName = "Vazba a aranžmá", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("b6fbb1f0-c86c-4c09-8c47-8973a536e818"), CategoryName = "Keře a stromy (Ovocné)", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue },
                    new Category { Id = Guid.Parse("11a4b47e-65c6-42eb-a1a0-b8e11a1f6c6e"), CategoryName = "Nástroje a nářadí", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue }
                );
        }
    }
}
