namespace Fei.Is.Api.Data.Models.InformationSystem
{
    public class UserFile : BaseModel
    {
        public string? LocalFileName { get; set; } = null;
        public string OriginalFileName { get; set; } = string.Empty;
        public FileIdentifier FileIdentifier { get; set; }
    }

    public enum FileIdentifier
    {
        QuotationSheet,
        Invoice,
        Order
    }
}
