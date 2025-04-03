namespace Fei.Is.Api.DocumentsGen.Templates
{
    public class TemplateLoader
    {
        public static string LoadTemplate(ETemplateType templateType)
        {
            System.Resources.ResourceReader resourceReader = new System.Resources.ResourceReader("Resources.resx");
            resourceReader.GetResourceData("Faktura", out string typeName, out byte[] data);
            return "";
        }
    }
}
