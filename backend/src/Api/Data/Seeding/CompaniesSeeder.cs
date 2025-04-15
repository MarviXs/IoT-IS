using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public class CompaniesSeeder : ISeed
    {
        public List<Type> GetDependencies()
        {
            return new List<Type>();
        }

        public Type GetModel()
        {
            return typeof(Company);
        }

        public void Seed(AppDbContext appDbContext)
        {
            appDbContext.Companies.AddRange(
                [
                    new Company
                    {
                        Id = Guid.NewGuid(),
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
                        Id = Guid.NewGuid(),
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
                        Id = Guid.NewGuid(),
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
                        Id = Guid.NewGuid(),
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
                        Id = Guid.NewGuid(),
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
                        Id = Guid.NewGuid(),
                        Title = "Jan Zatloukal - Zahradnictví Blešno",
                        Ic = "46212152",
                        Dic = "CZ6203071741",
                        Street = "Blešno 127",
                        Psc = "50346",
                        City = "Třebechovice pod Orebem",
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue
                    }
                ]
            );
        }
    }
}
