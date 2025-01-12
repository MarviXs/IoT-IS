using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Fei.Is.Api.DocumentsGen
{
    public class ExcelGenerator : IDocumentGen
    {
        public string ApplyFields(string documentPath, Dictionary<string, string> values)
        {
            IWorkbook wb = new XSSFWorkbook(documentPath);

            ISheet sheet = wb.GetSheetAt(0);

            foreach (IRow row in sheet)
            {
                foreach (ICell cell in row)
                {
                    if (values.ContainsKey(cell.StringCellValue))
                    {
                        cell.SetCellValue(values[cell.StringCellValue]);
                    }
                }
            }

            string newDocumentPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName(), Path.GetExtension(documentPath));

            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }

            return newDocumentPath;
        }
    }
}
