using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MathNet.Numerics.Distributions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public class ExcelGenerator : DocumentGen
    {
        public override string ApplyFields(string documentPath, JToken values)
        {
            IWorkbook wb = new XSSFWorkbook(documentPath);

            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                ISheet sheet = wb.GetSheetAt(i);

                ProcessSheet(sheet, values);
            }

            string newDocumentPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(documentPath));

            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }

            return newDocumentPath;
        }


        private void ProcessSheet(ISheet sheet, JToken values)
        {
            List<TemplateList> listTags = new List<TemplateList>();

            foreach (IRow row in sheet)
            {
                foreach (ICell cell in row)
                {
                    if (cell.CellType != CellType.String)
                        continue;

                    if (Regex.IsMatch(cell.StringCellValue, REGEX_LIST_START_PATTERN))
                    {
                        TemplateList listTag = new TemplateList();
                        listTag.startRowIndex = cell.RowIndex;
                        listTag.tag = Regex.Match(cell.StringCellValue, REGEX_LIST_START_PATTERN).Groups[1].Value;
                        listTags.Add(listTag);
                    }

                    if (Regex.IsMatch(cell.StringCellValue, REGEX_LIST_END_PATTERN))
                    {
                        TemplateList? listTag = listTags.Find(tag => tag.tag == Regex.Match(cell.StringCellValue, REGEX_LIST_END_PATTERN).Groups[1].Value);
                        if (listTag is not null)
                        {
                            listTag.endRowIndex = cell.RowIndex;
                        }
                    }
                } 
            }

            foreach (TemplateList listTag in listTags)
            {
                JToken? listValues = values.SelectToken(listTag.tag);

                if (listValues is null)
                {
                    continue;
                }

                FillList(sheet, listValues, listTag.startRowIndex, listTag.endRowIndex);
            }

            foreach (IRow row in sheet)
            {
                foreach (ICell cell in row)
                {
                    if (cell.CellType == CellType.String && Regex.IsMatch(cell.StringCellValue, REGEX_ITEM_PATTERN))
                    {
                        string renderedString = StubbleRenderer.Render(cell.StringCellValue, values);
                        cell.SetCellValue(renderedString);
                    }
                }
            }
        }

        private void FillList(ISheet sheet, JToken values, int searchStartRowIndex, int searchEndRoxIndex)
        {
            ICell? listStartTemplate = sheet.GetRow(searchStartRowIndex).Cells.Find(cell => Regex.IsMatch(cell.StringCellValue, REGEX_LIST_START_PATTERN));

            if (listStartTemplate == null)
            {
                return;
            }

            for (int i = 1; i <= values.Count(); i++)
            {
                IRow newRow = sheet.CopyRow(searchStartRowIndex + 1, searchStartRowIndex + 1 + i);

                foreach(ICell cell in newRow)
                {
                    if (cell != null && cell.CellType == CellType.String && Regex.IsMatch(cell.StringCellValue, REGEX_ITEM_PATTERN))
                    {
                        string renderedString = StubbleRenderer.Render(cell.StringCellValue, values[i - 1]);
                        cell.SetCellValue(renderedString);
                    }
                }
            }

            sheet.RemoveRow(sheet.GetRow(searchStartRowIndex));
            sheet.ShiftRows(searchStartRowIndex + 1, sheet.LastRowNum, -1);

            sheet.RemoveRow(sheet.GetRow(searchStartRowIndex));
            sheet.ShiftRows(searchStartRowIndex + 1, sheet.LastRowNum, -1);

            sheet.RemoveRow(sheet.GetRow(searchEndRoxIndex + values.Count() - 2));
            sheet.ShiftRows(searchEndRoxIndex - 1 + values.Count(), sheet.LastRowNum, -1);
        }
    }
}
