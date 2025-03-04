namespace Fei.Is.Api.DocumentsGen.Generators
{
    public abstract class DocumentGen
    {
        protected string regexPattern = @"{{(.*?)}}";
        public abstract string ApplyFields(string documentPath, Dictionary<string, string> values);
        public abstract List<string> GetFields(string documentPath);
    }
}
