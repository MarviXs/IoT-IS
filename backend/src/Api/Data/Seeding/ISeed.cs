using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Data.Seeding
{
    public interface ISeed
    {
        public void Seed(AppDbContext dbContext);
        public List<Type> GetDependencies();
        public Type GetModel();
    }
}
