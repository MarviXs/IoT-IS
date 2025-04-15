using System;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using NPOI.POIFS.Storage;

namespace Fei.Is.Api.Data.Seeding
{
    public class ProductSeeder : ISeed
    {
        public List<Type> GetDependencies()
        {
            return new List<Type>() { typeof(Supplier), typeof(Category), typeof(VATCategory) };
        }

        public Type GetModel()
        {
            return typeof(Product);
        }

        public void Seed(AppDbContext dbContext)
        {
            List<Supplier> suppliers = dbContext.Suppliers.Take(5).ToList();

            if (suppliers.Count < 5)
            {
                throw new Exception("Not enough suppliers in the database");
            }

            List<Category> categories = dbContext.Categories.Take(4).ToList();

            if (categories.Count < 4)
            {
                throw new Exception("Not enough categories in the database");
            }

            List<VATCategory> vatCategories = dbContext.VATCategories.Take(2).ToList();

            if (vatCategories.Count < 2)
            {
                throw new Exception("Not enough VAT categories in the database");
            }

            dbContext.Products.AddRange(
                new Product()
                {
                    Id = Guid.NewGuid(),
                    PLUCode = "PLU20001",
                    Code = "CODE201",
                    LatinName = "Rosa rubiginosa",
                    CzechName = "Růže červená",
                    FlowerLeafDescription = "Elegantné červené kvety",
                    PotDiameterPack = "10 cm",
                    PricePerPiecePack = 3.50m,
                    DiscountedPriceWithoutVAT = 3.00m,
                    RetailPrice = 4.00m,
                    Variety = "Červená",
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    Supplier = suppliers[0],
                    Category = categories[0],
                    VATCategory = vatCategories[0]
                },
                new Product()
                {
                    Id = Guid.NewGuid(),
                    PLUCode = "PLU11001",
                    Code = "CODE201",
                    LatinName = "Rašelina bílá",
                    CzechName = "AGRO",
                    FlowerLeafDescription = "",
                    PotDiameterPack = "180l",
                    PricePerPiecePack = 448.5m,
                    DiscountedPriceWithoutVAT = 403.65m,
                    RetailPrice = 733m,
                    Variety = "Červená",
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    Supplier = suppliers[1],
                    Category = categories[2],
                    VATCategory = vatCategories[1]
                },
                new Product()
                {
                    Id = Guid.NewGuid(),
                    PLUCode = "PLU8031",
                    Code = "8592000003522",
                    LatinName = "Narcisus pseudonarcisus",
                    CzechName = "Narcis jarní",
                    FlowerLeafDescription = "Různé odrůdy,  3ks",
                    PotDiameterPack = "14 cm",
                    PricePerPiecePack = 56m,
                    DiscountedPriceWithoutVAT = 50.4m,
                    RetailPrice = 87m,
                    Variety = "Červená",
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    Supplier = suppliers[2],
                    Category = categories[3],
                    VATCategory = vatCategories[0]
                }
            );
        }
    }
}
