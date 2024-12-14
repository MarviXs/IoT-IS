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
                        //Id = 1,
                        Title = "Petunia s.r.o.",
                        Title2 = null,
                        Ic = "25735641",
                        Dic = "CZ25735641",
                        Ulice = "Třemošenská 658",
                        Psc = "26101",
                        City = "Příbram"
                    },
                    new Company
                    {
                        Id = Guid.Parse("7337c6db-43d7-4c10-aeb3-3ef2f853f7d3"),
                        //Id = 2,
                        Title = "Štamberková Monika",
                        Title2 = null,
                        Ic = "71070877",
                        Dic = "CZ696123003",
                        Ulice = "K Vršíčku 91",
                        Psc = "26202",
                        City = "Stará Huť"
                    },
                    new Company
                    {
                        Id = Guid.Parse("f51a5725-b267-4c19-9cf0-444bb7c32b6e"),
                        //Id = 3,
                        Title = "Arboeko s.r.o.",
                        Title2 = null,
                        Ic = "27469613",
                        Dic = "CZ27469613",
                        Ulice = "Lhota 244",
                        Psc = "27714",
                        City = "Dřísy"
                    },
                    new Company
                    {
                        Id = Guid.Parse("91827187-f264-44b2-b6e3-697a752aa968"),
                        //Id = 4,
                        Title = "Moravol s.r.o.",
                        Title2 = null,
                        Ic = "28282711",
                        Dic = "CZ28282711",
                        Ulice = "Olbramkostel 41",
                        Psc = "67151",
                        City = "Olbramkostel"
                    },
                    new Company
                    {
                        Id = Guid.Parse("be97065a-c6e9-4b03-a173-c1c85f9b42db"),
                        //Id = 5, 
                        Title = "LM Agroton s.r.o.",
                        Title2 = null,
                        Ic = "29296721",
                        Dic = "CZ29296721",
                        Ulice = "Mírová 407",
                        Psc = "79312",
                        City = "Horní Benešov"
                    },
                    new Company
                    {
                        Id = Guid.Parse("3b29e227-bb97-441f-878e-d4b1111b3ebb"),
                        //Id = 6,
                        Title = "Jan Zatloukal - Zahradnictví Blešno",
                        Title2 = null,
                        Ic = "46212152",
                        Dic = "CZ6203071741",
                        Ulice = "Blešno 127",
                        Psc = "50346",
                        City = "Třebechovice pod Orebem"
                    }
                );
        }
    }
}
