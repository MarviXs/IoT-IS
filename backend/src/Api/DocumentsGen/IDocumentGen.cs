namespace Fei.Is.Api.DocumentsGen
{
    public interface IDocumentGen
    {
        public string ApplyFields(string documentPath, Dictionary<string, string> values);
    }
}
