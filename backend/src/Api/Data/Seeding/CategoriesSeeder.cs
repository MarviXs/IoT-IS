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
                    new Category { Id = Guid.NewGuid(), CategoryName = "BALKÓNOVÉ ROSTLINY, LETNIČKY, DVOULETKY, TRVALKY A TRÁVY" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "POKOJOVÉ A PŘENOSNÉ ROSTLINY" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "OKURKY ROUBOVANÉ, Pravokořenné" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "TYKVE - CUKETY" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "RAJČATA, LILEK" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Papriky" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "LISTOVÁ ZELENINA" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "BYLINKY" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "CHRYZANTÉMY" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Podzimní košík s květinami" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Vřes" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Cibuloviny" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Substráty, hnojiva a ostatní materiály" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Osiva" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Keře a stromy (Okrasné)" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "DENIVKY A IRISY" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Vazba a aranžmá" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Keře a stromy (Ovocné)" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Nástroje a nářadí" }
                );
        }
    }
}
