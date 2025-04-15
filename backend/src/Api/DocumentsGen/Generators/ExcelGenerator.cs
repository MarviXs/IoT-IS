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
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using Stubble.Core.Interfaces;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public class ExcelGenerator : DocumentGen
    {
        private char decimalSeparator = Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        private short numberCellDataFormat;
        private short dateCellDataFormat;
        public override string ApplyFields(FileStream fileStream, string newFileName, JToken values)
        {
            XSSFWorkbook wb = new XSSFWorkbook(fileStream);

            numberCellDataFormat = wb.CreateDataFormat().GetFormat($"0{decimalSeparator}00");
            dateCellDataFormat = wb.CreateDataFormat().GetFormat("dd/MM/yyyy");

            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                ISheet sheet = wb.GetSheetAt(i);

                ProcessSheet(sheet, values, sheet.GetRow(sheet.FirstRowNum), sheet.GetRow(sheet.LastRowNum));
            }

            XSSFFormulaEvaluator.EvaluateAllFormulaCells(wb);

            string newDocumentPath = Path.Combine(Path.GetTempPath(), newFileName);
            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }

            return newDocumentPath;
        }

        private void ProcessSheet(ISheet sheet, JToken values, IRow startRow, IRow endRow)
        {
            List<TemplateList> listTags = new List<TemplateList>();
            TemplateList? currentList = null;

            foreach (IRow row in sheet)
            {
                if (row.RowNum < startRow.RowNum || row.RowNum > endRow.RowNum)
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
                        currentList.startRow = row;
                        currentList.tag = Regex.Match(cell.StringCellValue, REGEX_LIST_START_PATTERN).Groups[1].Value;
                    }

                    if (
                        Regex.IsMatch(cell.StringCellValue, REGEX_LIST_END_PATTERN)
                        && currentList != null
                        && currentList.tag == cell.StringCellValue.Substring(3, cell.StringCellValue.Length - 5)
                    )
                    {
                        currentList.endRow = row;
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
                if (row.RowNum < startRow.RowNum || row.RowNum > endRow.RowNum)
                {
                    continue;
                }

                foreach (ICell cell in row)
                {
                    if (cell.CellType == CellType.String && Regex.IsMatch(cell.StringCellValue, REGEX_ITEM_PATTERN))
                    {
                        FillCell(values, cell, cell.StringCellValue);
                    }
                }
            }
        }

        private void CopyList(ISheet sheet, JToken values, TemplateList list)
        {
            List<IRow> templateRows = new List<IRow>();

            for (int i = list.startRow.RowNum + 1; i < list.endRow.RowNum; i++)
            {
                templateRows.Add(sheet.GetRow(i));
            }

            int innerListStartIndex = -1;

            List<List<IRow>> innerLists = new List<List<IRow>>();

            for (int currentObjectIndex = 0; currentObjectIndex < values.Count(); currentObjectIndex++)
            {
                List<IRow> innerListRows = new List<IRow>();
                if (innerListStartIndex == -1)
                {
                    innerListStartIndex = list.endRow.RowNum;
                }
                else
                {
                    innerListStartIndex = innerLists.Last().Last().RowNum + 1;
                }

                for (int currentInnerRowIndex = 0; currentInnerRowIndex < templateRows.Count; currentInnerRowIndex++)
                {
                    IRow newRow = sheet.CopyRow(templateRows[currentInnerRowIndex].RowNum, innerListStartIndex + currentInnerRowIndex);
                    innerListRows.Add(newRow);
                }

                innerLists.Add(innerListRows);
            }

            for (int i = 0; i < innerLists.Count; i++)
            {
                ProcessSheet(sheet, values[i], innerLists[i].First(), innerLists[i].Last());
                foreach (IRow row in innerLists[i])
                {
                    foreach (ICell cell in row)
                    {
                        FillListCell(cell, values[i]);
                    }
                }
            }

            DeleteRow(sheet, list.startRow);

            templateRows.ForEach(row => DeleteRow(sheet, row));

            DeleteRow(sheet, list.endRow);
        }

        private void FillListCell(ICell cell, JToken? values)
        {
            if (values is null)
            {
                return;
            }
            if (cell != null && cell.CellType == CellType.String && Regex.IsMatch(cell.StringCellValue, REGEX_LIST_ITEM_PATTERN))
            {
                string cellValue = cell.StringCellValue;

                cellValue = cellValue.Remove(cellValue.IndexOf("{{") + 2, 1);

                FillCell(values, cell, cellValue);
            }
        }

        private void FillCell(JToken? values, ICell cell, string cellValue)
        {
            if (values is null)
            {
                return;
            }

            string renderedString = StubbleRenderer.Render(cellValue, values);
            if (double.TryParse(renderedString, out double doubleValue))
            {
                cell.SetCellValue(doubleValue);
                cell.CellStyle.DataFormat = numberCellDataFormat;
            }
            else if (DateTime.TryParse(renderedString, out DateTime dateValue))
            {
                cell.SetCellValue(dateValue);
                cell.CellStyle.DataFormat = dateCellDataFormat;
            }
            else
            {
                cell.SetCellValue(renderedString);
            }
        }

        private void DeleteRow(ISheet sheet, IRow row)
        {
            sheet.ShiftRows(row.RowNum + 1, sheet.LastRowNum, -1);
        }
    }
}
