using System.Text.RegularExpressions;
using MathNet.Numerics.Distributions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public class ExcelGenerator : DocumentGen
    {
        public override string ApplyFields(string documentPath, Dictionary<string, object> values)
        {
            IWorkbook wb = new XSSFWorkbook(documentPath);

            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                ISheet sheet = wb.GetSheetAt(i);

                ProcessSheet(sheet, StubbleRenderer, values, "");
            }

            string newDocumentPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(documentPath));

            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }

            return newDocumentPath;
        }

        private void ProcessSheet(ISheet sheet, JObject data, int searchStartRowIndex, int searchEndRoxIndex, string currentPath)
        {
            int startRow = -1;
            int endRow = -1;
            string currentListTag = string.Empty;

            // Find the main list block {{#Orders}} and {{/Orders}}
            for (int row = searchStartRowIndex; row <= searchStartRowIndex; row++)
            {
                IRow currentRow = sheet.GetRow(row);

                if (currentRow == null)
                {
                    continue;
                }

                currentRow.Cells.ForEach(cell =>
                {
                    if (cell != null && cell.CellType == CellType.String)
                    {
                        string cellValue = cell.StringCellValue;
                        if (startRow == -1 && Regex.IsMatch(cellValue, REGEX_LIST_START_PATTERN))
                        {
                            currentListTag = Regex.Match(cellValue, REGEX_LIST_START_PATTERN).Groups[1].Value;
                            startRow = row;
                        }
                        if (endRow == -1 && cellValue.Equals(String.Format(REGEX_LIST_END_FORMAT, currentListTag)))
                        {
                            endRow = row;
                        }
                    }
                });
            }

            if (startRow == -1 || endRow == -1)
            {
                Console.WriteLine("⚠️ Missing list placeholders in template.");
                return;
            }

            for (int nestedListStartIndex = startRow + 1; nestedListStartIndex < endRow; nestedListStartIndex++)
            {
                IRow row = sheet.GetRow(nestedListStartIndex);
                if (row != null)
                {
                    for (int j = 0; j < row.LastCellNum; j++)
                    {
                        ICell cell = row.GetCell(j);
                        if (cell == null || cell.CellType != CellType.String)
                        {
                            continue;
                        }

                        string cellValue = cell.StringCellValue;
                        if (Regex.IsMatch(cellValue, REGEX_LIST_START_FORMAT))
                        {
                            string otherTag = Regex.Match(cellValue, REGEX_LIST_START_PATTERN).Groups[1].Value;
                            ProcessSheet(sheet, data, nestedListStartIndex, endRow - 1, $"{currentPath}.{currentListTag}");
                        }
                    }
                }
            }

            // Extract order template
            List<IRow> orderTemplate = new List<IRow>();
            for (int i = startRow + 1; i < endRow; i++)
            {
                orderTemplate.Add(sheet.GetRow(i));
            }

            // Remove template rows
            for (int i = endRow; i >= startRow; i--)
            {
                sheet.RemoveRow(sheet.GetRow(i));
            }

            var currentObject = data;
            int insertPos = startRow;

            foreach (var order in orders)
            {
                foreach (IRow templateRow in orderTemplate)
                {
                    IRow newRow = sheet.CreateRow(insertPos++);
                    CopyRow(templateRow, newRow);

                    // Render placeholders
                    for (int col = 0; col < templateRow.LastCellNum; col++)
                    {
                        ICell cell = newRow.GetCell(col);
                        if (cell != null && cell.CellType == CellType.String)
                        {
                            cell.SetCellValue(stubble.Render(cell.StringCellValue, order));
                        }
                    }

                    // Check if this row contains nested `{{#Items}}`
                    if (templateRow.GetCell(0) != null && templateRow.GetCell(0).StringCellValue.Contains("{{#Items}}"))
                    {
                        int itemsStartRow = insertPos - 1; // The row where items should start
                        int itemsEndRow = FindEndIndex(orderTemplate, "{{/Items}}");

                        if (itemsEndRow == -1)
                            continue; // No nested list found

                        // Extract items template rows
                        List<IRow> itemsTemplate = new List<IRow>();
                        for (int i = itemsStartRow + 1; i < itemsEndRow + startRow; i++)
                        {
                            itemsTemplate.Add(sheet.GetRow(i));
                        }

                        var items = (List<Dictionary<string, object>>)order["Items"];
                        int itemInsertPos = insertPos;

                        foreach (var item in items)
                        {
                            foreach (IRow itemTemplateRow in itemsTemplate)
                            {
                                IRow newItemRow = sheet.CreateRow(itemInsertPos++);
                                CopyRow(itemTemplateRow, newItemRow);

                                for (int col = 0; col < itemTemplateRow.LastCellNum; col++)
                                {
                                    ICell cell = newItemRow.GetCell(col);
                                    if (cell != null && cell.CellType == CellType.String)
                                    {
                                        cell.SetCellValue(stubble.Render(cell.StringCellValue, item));
                                    }
                                }
                            }
                        }

                        insertPos = itemInsertPos; // Update position after inserting items
                    }
                }
            }
        }
    }
}
