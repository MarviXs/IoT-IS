using NPOI.OpenXml4Net.OPC;
using NPOI.XWPF.UserModel;

namespace Fei.Is.Api.DocumentsGen
{
    public class WordGenerator : IDocumentGen
    {
        public string ApplyFields(string documentPath, Dictionary<string, string> values)
        {
            XWPFDocument document = new XWPFDocument(OPCPackage.Open(documentPath));

            foreach (KeyValuePair<string, string> item in values)
            {
                document.FindAndReplaceText(item.Key, item.Value);
            }

            string newDocumentPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName(), Path.GetExtension(documentPath));

            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                document.Write(fs);
            }

            return newDocumentPath;
        }
    }
}
