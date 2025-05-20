using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public class ExcelGenerator : DocumentGen
    {
        private char decimalSeparator = Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        private short numberCellDataFormat;
        private short dateCellDataFormat;

        public override string ApplyFields(FileStream fileStream, string newFileName, JToken values, Dictionary<string, string>? images = null)
        {
            XSSFWorkbook wb = new XSSFWorkbook(fileStream);

            numberCellDataFormat = wb.CreateDataFormat().GetFormat($"0{decimalSeparator}00");
            dateCellDataFormat = wb.CreateDataFormat().GetFormat("dd/MM/yyyy");

            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                ISheet sheet = wb.GetSheetAt(i);

                ProcessSheet(sheet, values, sheet.GetRow(sheet.FirstRowNum), sheet.GetRow(sheet.LastRowNum));

                var draw = sheet.DrawingPatriarch as XSSFDrawing;

                if (draw == null)
                    continue;

                var shapes = draw?.GetShapes();

                ProcessImages(sheet, shapes, wb, images);
            }

            XSSFFormulaEvaluator.EvaluateAllFormulaCells(wb);

            string newDocumentPath = Path.Combine(Path.GetTempPath(), newFileName);
            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }

            return newDocumentPath;
        }

        private void ProcessImages(ISheet sheet, List<XSSFShape> shapes, IWorkbook wb, Dictionary<string, string> images)
        {
            if (shapes.Count == 0)
                return;

            List<XSSFPicture> pictures = shapes.Where(shape => shape is XSSFPicture).Select(shape => (XSSFPicture)shape).ToList();

            XSSFCreationHelper creationHelper = wb.GetCreationHelper() as XSSFCreationHelper;

            foreach (IRow row in sheet)
            {
                foreach (ICell cell in row)
                {
                    if (cell.CellType == CellType.String && Regex.IsMatch(cell.StringCellValue, REGEX_IMAGE_PATTERN))
                    {
                        string cellValue = cell.StringCellValue;
                        string imageName = Regex.Match(cellValue, REGEX_IMAGE_PATTERN).Groups[1].Value;

                        XSSFPicture picture = pictures.FirstOrDefault(p => p.ShapeName.Equals(imageName));

                        if (picture is null)
                            continue;

                        XSSFClientAnchor anchor = creationHelper.CreateClientAnchor() as XSSFClientAnchor;
                        anchor.Col1 = cell.ColumnIndex;
                        anchor.Col2 = cell.ColumnIndex + picture.ClientAnchor.Col2 - picture.ClientAnchor.Col1;
                        anchor.Row1 = cell.RowIndex;
                        anchor.Row2 = cell.RowIndex + picture.ClientAnchor.Row2 - picture.ClientAnchor.Row1;
                        anchor.Dx1 = picture.ClientAnchor.Dx1;
                        anchor.Dx2 = picture.ClientAnchor.Dx2;
                        anchor.Dy1 = picture.ClientAnchor.Dy1;
                        anchor.Dy2 = picture.ClientAnchor.Dy2;

                        int pictureIndex = -1;
                        if (images is not null && images.ContainsKey(imageName))
                        {
                            string imagePath = images[imageName];
                            using FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                            byte[] imageData = new byte[imageStream.Length];
                            imageStream.Read(imageData, 0, (int)imageStream.Length);
                            pictureIndex = wb.AddPicture(imageData, picture.PictureData.PictureType);

                            imageStream.Close();
                            File.Delete(imagePath);
                        }
                        else
                        {
                            pictureIndex = wb.AddPicture(picture.PictureData.Data, picture.PictureData.PictureType);
                        }
                        sheet.DrawingPatriarch.CreatePicture(anchor, pictureIndex);

                        cell.SetCellValue("");
                    }
                }
            }

            foreach (XSSFPicture xssfPicture in pictures)
            {
                XSSFDrawing drawing = xssfPicture.GetDrawing();
                String rId = xssfPicture.GetCTPicture().blipFill.blip.embed;
                drawing.GetPackagePart().RemoveRelationship(rId);
                drawing.GetPackagePart().Package.DeletePartRecursive(drawing.GetRelationById(rId).GetPackagePart().PartName);

                drawing.GetCTDrawing().CellAnchors.RemoveAll(x => x.picture.blipFill.blip.embed == rId);
            }
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
                if (Regex.IsMatch(cell.StringCellValue, REGEX_IMAGE_PATTERN))
                    return;

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
                if (Regex.IsMatch(cellValue, REGEX_IMAGE_PATTERN))
                    return;

                cell.SetCellValue(renderedString);
            }
        }

        private void DeleteRow(ISheet sheet, IRow row)
        {
            if (row.RowNum == sheet.LastRowNum)
            {
                sheet.RemoveRow(row);
            }
            else
            {
                sheet.ShiftRows(row.RowNum + 1, sheet.LastRowNum, -1);
            }
        }
    }
}
