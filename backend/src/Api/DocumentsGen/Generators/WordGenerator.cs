using NPOI.HSSF.UserModel;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System.Text.RegularExpressions;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public class WordGenerator : DocumentGen
    {
        public override string ApplyFields(string documentPath, Dictionary<string, string> values)
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

        public override List<string> GetFields(string documentPath)
        {
            throw new NotImplementedException();
        }
    }
}
