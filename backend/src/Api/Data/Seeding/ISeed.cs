using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public interface ISeed
    {
        public void Seed(ModelBuilder modelBuilder);
    }
}
