namespace Fei.Is.Api.Data.Models.InformationSystem
{
    public class VATCategory : BaseModel
    {
        public required string Name { get; set; }
        public required decimal Rate { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
