using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MathNet.Numerics.Distributions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.POIFS.NIO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Stubble.Core.Interfaces;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public class ExcelGenerator : DocumentGen
    {
        public override string ApplyFields(string documentPath, JToken values)
        {
            XSSFWorkbook wb;

            using (FileStream fs = new FileStream(documentPath, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(fs);
            }

            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                ISheet sheet = wb.GetSheetAt(i);

                ProcessSheet(sheet, values, new TemplateRow(sheet.GetRow(sheet.FirstRowNum)), new TemplateRow(sheet.GetRow(sheet.LastRowNum)));
            }

            string newDocumentPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(documentPath));
            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }

            return newDocumentPath;
        }


        private void ProcessSheet(ISheet sheet, JToken values, TemplateRow startRow, TemplateRow endRow)
        {
            List<TemplateList> listTags = new List<TemplateList>();
            TemplateList? currentList = null;

            foreach (IRow row in sheet)
            {
                if (row.RowNum < startRow.Row.RowNum || row.RowNum > endRow.Row.RowNum)
                {
                    continue;
                }

                foreach (ICell cell in row)
                {
                    if (cell.CellType != CellType.String)
                        continue;

                    if (Regex.IsMatch(cell.StringCellValue, REGEX_LIST_START_PATTERN) && currentList is null)
                    {
                        currentList = new TemplateList();
                        currentList.startRow = new TemplateRow(row);
                        currentList.tag = Regex.Match(cell.StringCellValue, REGEX_LIST_START_PATTERN).Groups[1].Value;
                    }

                    if (Regex.IsMatch(cell.StringCellValue, REGEX_LIST_END_PATTERN) &&
                        currentList != null &&
                        currentList.tag == cell.StringCellValue.Substring(3, cell.StringCellValue.Length - 5))
                    {
                        currentList.endRow = new TemplateRow(row);
                        listTags.Add(currentList);
                        currentList = null;
                    }
                }
            }

            foreach (TemplateList list in listTags)
            {
                JToken? listValues = values.SelectToken(list.tag);

                if (listValues is null)
                {
                    continue;
                }

                CopyList(sheet, listValues, list);
            }

            foreach (IRow row in sheet)
            {
                if (row.RowNum < startRow.Row.RowNum || row.RowNum > endRow.Row.RowNum)
                {
                    continue;
                }

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

        private void CopyList(ISheet sheet, JToken values, TemplateList list)
        {
            List<TemplateRow> innerTemplateRows = new List<TemplateRow>();

            for (int i = list.startRow.Row.RowNum + 1; i < list.endRow.Row.RowNum; i++)
            {
                innerTemplateRows.Add(new TemplateRow(sheet.GetRow(i)));
            }

            for (int currentObjectIndex = 0; currentObjectIndex < values.Count(); currentObjectIndex++)
            {
                int innerListStartIndex = list.endRow.Row.RowNum + 1 + currentObjectIndex * innerTemplateRows.Count;

                for (int currentInnerRowIndex = 0; currentInnerRowIndex < innerTemplateRows.Count; currentInnerRowIndex++)
                {
                    IRow newRow = sheet.CopyRow(innerTemplateRows[currentInnerRowIndex].Row.RowNum, innerListStartIndex + currentInnerRowIndex);

                    list.insertedRows.Add(new TemplateRow(newRow));
                }

                ProcessSheet(sheet, values[currentObjectIndex], list.insertedRows.First(), list.insertedRows.Last());

                foreach (TemplateRow row in list.insertedRows)
                {
                    if (row.deleted)
                    {
                        continue;
                    }

                    foreach (ICell cell in row.Row)
                    {
                        FillListCell(cell, values[currentObjectIndex]);
                    }
                }
            }

            DeleteRow(sheet, list.startRow);
            list.startRow.deleted = true;

            innerTemplateRows.ForEach(row => DeleteRow(sheet, row));

            DeleteRow(sheet, list.endRow);
            list.startRow.deleted = true;
        }

        private void FillListCell(ICell cell, JToken? values)
        {
            if (values is null)
            {
                return;
            }
            if (cell != null && cell.CellType == CellType.String && Regex.IsMatch(cell.StringCellValue, REGEX_LIST_ITEM_PATTERN))
            {
                string renderedString = StubbleRenderer.Render(cell.StringCellValue.Remove(2, 1), values);
                cell.SetCellValue(renderedString);
            }
        }

        private void DeleteRow(ISheet sheet, TemplateRow row)
        {
            sheet.ShiftRows(row.Row.RowNum + 1, sheet.LastRowNum, -1);
            row.deleted = true;
        }
    }
}
