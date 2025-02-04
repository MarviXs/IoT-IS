using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public class CompaniesSeeder : ISeed
    {
        public void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Company>()
                .HasData(
                    new Company
                    {
                        Id = Guid.Parse("479b6c63-f552-4a6e-b706-62ec96edb896"),
                        Title = "Petunia s.r.o.",
                        Ic = "25735641",
                        Dic = "CZ25735641",
                        Street = "Třemošenská 658",
                        Psc = "26101",
                        City = "Příbram",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue
                    },
                    new Company
                    {
                        Id = Guid.Parse("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                        Title = "Štamberková Monika",
                        Ic = "71070877",
                        Dic = "CZ696123003",
                        Street = "K Vršíčku 91",
                        Psc = "26202",
                        City = "Stará Huť",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue
                    },
                    new Company
                    {
                        Id = Guid.Parse("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                        Title = "Arboeko s.r.o.",
                        Ic = "27469613",
                        Dic = "CZ27469613",
                        Street = "Lhota 244",
                        Psc = "27714",
                        City = "Dřísy",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue
                    },
                    new Company
                    {
                        Id = Guid.Parse("91827187-f264-44b2-b6e3-697a752aa968"),
                        Title = "Moravol s.r.o.",
                        Ic = "28282711",
                        Dic = "CZ28282711",
                        Street = "Olbramkostel 41",
                        Psc = "67151",
                        City = "Olbramkostel",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue
                    },
                    new Company
                    {
                        Id = Guid.Parse("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                        Title = "LM Agroton s.r.o.",
                        Ic = "29296721",
                        Dic = "CZ29296721",
                        Street = "Mírová 407",
                        Psc = "79312",
                        City = "Horní Benešov",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue
                    },
                    new Company
                    {
                        Id = Guid.Parse("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                        Title = "Jan Zatloukal - Zahradnictví Blešno",
                        Ic = "46212152",
                        Dic = "CZ6203071741",
                        Street = "Blešno 127",
                        Psc = "50346",
                        City = "Třebechovice pod Orebem",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue
                    }
                );
        }
    }
}
