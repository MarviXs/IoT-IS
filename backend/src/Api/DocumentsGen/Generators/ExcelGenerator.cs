using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public class ExcelGenerator : DocumentGen
    {
        public override string ApplyFields(string documentPath, Dictionary<string, string> values)
        {
            IWorkbook wb = new XSSFWorkbook(documentPath);

            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                ISheet sheet = wb.GetSheetAt(i);

                foreach (IRow row in sheet)
                {
                    foreach (ICell cell in row)
                    {
                        if (cell.CellType == CellType.String && values.ContainsKey(cell.StringCellValue))
                        {
                            cell.SetCellValue(values[cell.StringCellValue]);
                        }
                    }
                }
            }

            

            string newDocumentPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(documentPath));

            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }

            return newDocumentPath;
        }

        public override List<string> GetFields(string documentPath)
        {
            List<string> matches = new List<string>();
            IWorkbook workbook;

            using (FileStream fs = new FileStream(documentPath, FileMode.Open, FileAccess.Read))
            {
                if (Path.GetExtension(documentPath).ToLower() == ".xls")
                    workbook = new HSSFWorkbook(fs); // For .xls files (older Excel format)
                else
                    workbook = new XSSFWorkbook(fs); // For .xlsx files (newer Excel format)
            }

            Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);

                for (int rowNum = 0; rowNum <= sheet.LastRowNum; rowNum++)
                {
                    IRow row = sheet.GetRow(rowNum);
                    if (row == null) continue;

                    for (int cellNum = 0; cellNum < row.LastCellNum; cellNum++)
                    {
                        ICell cell = row.GetCell(cellNum);
                        if (cell == null || cell.CellType != CellType.String) continue;

                        string cellValue = cell.StringCellValue;
                        if (regex.IsMatch(cellValue))
                        {
                            string match = regex.Match(cellValue).Value;

                            matches.Add(match);
                        }
                    }

                }
            }

            return matches;
        }
    }
}
