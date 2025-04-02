using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;
using System;

namespace Fei.Is.Api.Data.Seeding
{
    public class ProductsSeeder : ISeed
    {
        public void Seed(ModelBuilder modelBuilder)
        {
            /* modelBuilder.Entity<Product>().HasData(
                new Product()
                {
                    Id = new Guid("21a51405-3c66-4e25-89ec-24f18e6232b9"),
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
                    CreatedAt = DateTime.Parse("2024-12-30T17:19:44.631861Z"),
                    UpdatedAt = DateTime.Parse("2024-12-30T17:19:44.631861Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("86a5d954-903b-4aca-8aa6-a04d7570b3bf"),
                    PLUCode = "PLU20002",
                    Code = "CODE202",
                    LatinName = "Tulipa gesneriana",
                    CzechName = "Tulipán žltý",
                    FlowerLeafDescription = "Jemné žlté okvetné lístky",
                    PotDiameterPack = "12 cm",
                    PricePerPiecePack = 2.80m,
                    DiscountedPriceWithoutVAT = 2.50m,
                    RetailPrice = 3.00m,
                    Variety = "Žltý",
                    CreatedAt = DateTime.Parse("2025-01-01T19:43:25.326687Z"),
                    UpdatedAt = DateTime.Parse("2025-01-01T19:43:25.326687Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("47de5103-0662-4f54-b61b-70617d33a7c3"),
                    PLUCode = "PLU10001",
                    Code = "CODE001",
                    LatinName = "Tulipa hybrida",
                    CzechName = "Tulipán",
                    FlowerLeafDescription = "Farebné okvetné lístky",
                    PotDiameterPack = "15 cm",
                    PricePerPiecePack = 2.50m,
                    DiscountedPriceWithoutVAT = 2.00m,
                    RetailPrice = 3.00m,
                    Variety = "Červený",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("5470fec6-ea5b-496d-9a0f-328f079cfd4b"),
                    PLUCode = "PLU10002",
                    Code = "CODE002",
                    LatinName = "Rosa canina",
                    CzechName = "Šípková růže",
                    FlowerLeafDescription = "Růžové okvetné lístky",
                    PotDiameterPack = "20 cm",
                    PricePerPiecePack = 3.00m,
                    DiscountedPriceWithoutVAT = 3.00m,
                    RetailPrice = 4.00m,
                    Variety = "Růžová",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("73d7ae5e-adc9-4bdd-81e0-cd3327ac4ad3"),
                    PLUCode = "PLU10003",
                    Code = "CODE003",
                    LatinName = "Lavandula angustifolia",
                    CzechName = "Levandule",
                    FlowerLeafDescription = "Fialové květy",
                    PotDiameterPack = "10 cm",
                    PricePerPiecePack = 1.50m,
                    DiscountedPriceWithoutVAT = 1.20m,
                    RetailPrice = 2.00m,
                    Variety = "Klasická",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    // Pre tento produkt sme použili inú kategóriu, napr. "Exotické kvetiny"
                    Category = new Category()
                    {
                        Id = new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"),
                        CategoryName = "Exotické kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("9963b555-7f7c-4261-81f5-c944d8aea416"),
                    PLUCode = "PLU10004",
                    Code = "CODE004",
                    LatinName = "Ficus elastica",
                    CzechName = "Fíkus",
                    FlowerLeafDescription = "Lesklé zelené listy",
                    PotDiameterPack = "25 cm",
                    PricePerPiecePack = 5.00m,
                    DiscountedPriceWithoutVAT = 4.50m,
                    RetailPrice = 6.00m,
                    Variety = "Gumovník",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"),
                        CategoryName = "Práca a hlina"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("5bfc3ed5-8874-4452-9043-22065fc00e29"),
                        Name = "Reduced VAT",
                        Rate = 10.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("71408731-72ce-4f3f-a5bc-32ee9ebabb3c"),
                    PLUCode = "PLU10005",
                    Code = "CODE005",
                    LatinName = "Chrysanthemum indicum",
                    CzechName = "Chryzantéma",
                    FlowerLeafDescription = "Farebné květy",
                    PotDiameterPack = "12 cm",
                    PricePerPiecePack = 3.00m,
                    DiscountedPriceWithoutVAT = 2.70m,
                    RetailPrice = 3.50m,
                    Variety = "Žlutá",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("8903dab1-363f-494b-909d-26acb17ad5c4"),
                    PLUCode = "PLU10006",
                    Code = "CODE006",
                    LatinName = "Hedera helix",
                    CzechName = "Břečťan",
                    FlowerLeafDescription = "Zelené popínavé listy",
                    PotDiameterPack = "18 cm",
                    PricePerPiecePack = 4.00m,
                    DiscountedPriceWithoutVAT = 3.80m,
                    RetailPrice = 4.50m,
                    Variety = "Klasický",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("94052ccf-6797-4351-ad43-5130cb6c4fbe"),
                        CategoryName = "Práca a hlina"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("5bfc3ed5-8874-4452-9043-22065fc00e29"),
                        Name = "Reduced VAT",
                        Rate = 10.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("c8d82b9e-3538-4b9f-a81f-1cf79d3c55fe"),
                    PLUCode = "PLU10007",
                    Code = "CODE007",
                    LatinName = "Orchidaceae",
                    CzechName = "Orchidej",
                    FlowerLeafDescription = "Exotické květy",
                    PotDiameterPack = "22 cm",
                    PricePerPiecePack = 8.00m,
                    DiscountedPriceWithoutVAT = 7.50m,
                    RetailPrice = 9.00m,
                    Variety = "Bílá",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("7df5fe3b-1bbf-4dc8-a108-5c6f931e0db4"),
                        CategoryName = "Exotické kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("e0f08dda-16b1-41da-ae34-ebb9c997540d"),
                    PLUCode = "PLU10008",
                    Code = "CODE008",
                    LatinName = "Cyclamen persicum",
                    CzechName = "Brambořík",
                    FlowerLeafDescription = "Barevné okvětní lístky",
                    PotDiameterPack = "14 cm",
                    PricePerPiecePack = 2.80m,
                    DiscountedPriceWithoutVAT = 2.50m,
                    RetailPrice = 3.20m,
                    Variety = "Růžový",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("d7c9b140-0cfb-43fd-9ad4-33201a1462aa"),
                    PLUCode = "PLU10009",
                    Code = "CODE009",
                    LatinName = "Begonia semperflorens",
                    CzechName = "Begónie",
                    FlowerLeafDescription = "Barevné květy",
                    PotDiameterPack = "16 cm",
                    PricePerPiecePack = 3.20m,
                    DiscountedPriceWithoutVAT = 3.00m,
                    RetailPrice = 3.80m,
                    Variety = "Červená",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                },
                new Product()
                {
                    Id = new Guid("c48897b1-7d5e-4c68-a80c-7f17aaa27c7e"),
                    PLUCode = "PLU10010",
                    Code = "CODE010",
                    LatinName = "Pelargonium hortorum",
                    CzechName = "Pelargonie",
                    FlowerLeafDescription = "Zářivé květy",
                    PotDiameterPack = "17 cm",
                    PricePerPiecePack = 4.50m,
                    DiscountedPriceWithoutVAT = 4.00m,
                    RetailPrice = 5.00m,
                    Variety = "Bordó",
                    CreatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    UpdatedAt = DateTime.Parse("2025-01-17T19:16:39.664115Z"),
                    Supplier = new Supplier()
                    {
                        Id = new Guid("e8391bf0-9dc4-4d2e-a3f0-d028833ce902"),
                        Name = "Default Supplier"
                    },
                    Category = new Category()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        CategoryName = "Kvetiny"
                    },
                    VATCategory = new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Standard VAT",
                        Rate = 21.0m
                    }
                }
            ); */
        }
    }
}
