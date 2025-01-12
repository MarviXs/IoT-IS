using Fei.Is.Api.Data.Models.InformationSystem;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public class SystemSettingsSeeder : ISeed
    {
        public void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemSetting>().HasData(new SystemSetting() { Key = "DocumentTemplatesLoadPath", Value = "./DocumentTemplates" });
        }
    }
}
