﻿using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public class VATCategoriesSeeder : ISeed
    {
        public void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<VATCategory>()
                .HasData(
                    new VATCategory()
                    {
                        Id = new Guid("5bfc3ed5-8874-4452-9043-22065fc00e29"),
                        Name = "Normal",
                        Rate = 21,
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    },
                    new VATCategory()
                    {
                        Id = new Guid("37b1c257-1401-4d79-9c4f-a206b0937fd2"),
                        Name = "Reduced",
                        Rate = 19,
                        CreatedAt = DateTime.MinValue,
                        UpdatedAt = DateTime.MinValue,
                    }
                );
        }
    }
}
