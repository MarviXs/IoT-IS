namespace Fei.Is.Api.Data.Models.InformationSystem
{
    public class DeliveryNote : BaseModel
    {
        public string DeliveryNumber { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal VatGroup15 { get; set; }
        public decimal VatGroup21 { get; set; }
        public decimal VatTotal { get; set; }
        public decimal TotalAmountWithVat { get; set; }
        public string Took { get; set; }
        public string Forwarded { get; set; }
        public string Note { get; set; }
        public string LicensePlate { get; set; }
        public Company Supplier { get; set; }
        public Company Customer { get; set; }
        public ICollection<DeliveryItem> DeliveryItems { get; set; }
    }
}
