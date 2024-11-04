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
                    new Category { Id = 1, CategoryName = "BALKÓNOVÉ ROSTLINY, LETNIČKY, DVOULETKY, TRVALKY A TRÁVY" },
                    new Category { Id = 2, CategoryName = "POKOJOVÉ A PŘENOSNÉ ROSTLINY" },
                    new Category { Id = 3, CategoryName = "OKURKY ROUBOVANÉ, Pravokořenné" },
                    new Category { Id = 4, CategoryName = "TYKVE - CUKETY" },
                    new Category { Id = 5, CategoryName = "RAJČATA, LILEK" },
                    new Category { Id = 6, CategoryName = "Papriky" },
                    new Category { Id = 7, CategoryName = "LISTOVÁ ZELENINA" },
                    new Category { Id = 8, CategoryName = "BYLINKY" },
                    new Category { Id = 9, CategoryName = "CHRYZANTÉMY" },
                    new Category { Id = 10, CategoryName = "Podzimní košík s květinami" },
                    new Category { Id = 11, CategoryName = "Vřes" },
                    new Category { Id = 12, CategoryName = "Cibuloviny" },
                    new Category { Id = 13, CategoryName = "Substráty, hnojiva a ostatní materiály" },
                    new Category { Id = 14, CategoryName = "Osiva" },
                    new Category { Id = 15, CategoryName = "Keře a stromy (Okrasné)" },
                    new Category { Id = 16, CategoryName = "DENIVKY A IRISY" },
                    new Category { Id = 17, CategoryName = "Vazba a aranžmá" },
                    new Category { Id = 18, CategoryName = "Keře a stromy (Ovocné)" },
                    new Category { Id = 19, CategoryName = "Nástroje a nářadí" }
                );
        }
    }
}
