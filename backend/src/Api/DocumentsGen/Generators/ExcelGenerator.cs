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
            XSSFWorkbook wb = new XSSFWorkbook(documentPath);

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
                        listTag.startRow = row;
                        listTag.tag = Regex.Match(cell.StringCellValue, REGEX_LIST_START_PATTERN).Groups[1].Value;
                        listTags.Add(listTag);
                    }

                    if (Regex.IsMatch(cell.StringCellValue, REGEX_LIST_END_PATTERN))
                    {
                        TemplateList? listTag = listTags.Find(tag => tag.tag == Regex.Match(cell.StringCellValue, REGEX_LIST_END_PATTERN).Groups[1].Value);
                        if (listTag is not null)
                        {
                            listTag.endRow = row;
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

                FillList(sheet, listValues, listTag.startRow, listTag.endRow);
            }

            /*foreach (IRow row in sheet)
            {
                foreach (ICell cell in row)
                {
                    if (cell.CellType == CellType.String && Regex.IsMatch(cell.StringCellValue, REGEX_ITEM_PATTERN))
                    {
                        string renderedString = StubbleRenderer.Render(cell.StringCellValue, values);
                        cell.SetCellValue(renderedString);
                    }
                }
            }*/
        }

        private void FillList(ISheet sheet, JToken values, IRow listStartRow, IRow listEndRow)
        {
            List<IRow> innerTemplateRows = new List<IRow>();

            for (int i = listStartRow.RowNum + 1; i < listEndRow.RowNum; i++)
            {
                innerTemplateRows.Add(sheet.GetRow(i));
            }

            for (int currentObjectIndex = 1; currentObjectIndex <= values.Count(); currentObjectIndex++)
            {
                int innerListStartIndex = listStartRow.RowNum + (currentObjectIndex) * innerTemplateRows.Count + 1;

                for (int currentInnerRowIndex = 0; currentInnerRowIndex < innerTemplateRows.Count; currentInnerRowIndex++)
                {
                    IRow newRow = sheet.CopyRow(innerTemplateRows[currentInnerRowIndex].RowNum, innerListStartIndex + currentInnerRowIndex);

                    foreach (ICell cell in newRow)
                    {
                        FillCell(cell, values[currentObjectIndex - 1]);
                    }
                }
            }

            DeleteRow(sheet, listStartRow);

            innerTemplateRows.ForEach(row => DeleteRow(sheet, row));

            DeleteRow(sheet, listEndRow);
        }

        private void FillCell(ICell cell, JToken? values)
        {
            if (values is null)
            {
                return;
            }
            if (cell != null && cell.CellType == CellType.String && Regex.IsMatch(cell.StringCellValue, REGEX_ITEM_PATTERN))
            {
                string renderedString = StubbleRenderer.Render(cell.StringCellValue, values);
                cell.SetCellValue(renderedString);
            }
        }

        private void DeleteRow(ISheet sheet, IRow row)
        {
            sheet.ShiftRows(row.RowNum + 1, sheet.LastRowNum, -1);
        }
    }
}
