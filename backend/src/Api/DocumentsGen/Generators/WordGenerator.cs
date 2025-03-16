using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.Formula;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using Stubble.Core.Builders;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public class WordGenerator : DocumentGen
    {
        public override string ApplyFields(string documentPath, Dictionary<string, string> values)
        {
            /*XWPFDocument document = new XWPFDocument(OPCPackage.Open(documentPath));

            var stubble = new StubbleBuilder().Build();

            // Process each paragraph
            document
                .Paragraphs.Where(par => par.Text.Contains(REGEX_LIST_START_PATTERN))
                .ToList()
                .ForEach(par =>
                {
                    int startIdx = i;
                    int endIdx = startIdx;
                    while (endIdx < document.Paragraphs.Count && !document.Paragraphs[endIdx].Text.Contains(REGEX_LIST_END_PATTERN))
                    {
                        endIdx++;
                    }

                });

            for (int i = 0; i < document.Paragraphs.Count; i++)
            {
                XWPFParagraph para = document.Paragraphs[i];
                if (para.Text.Contains(REGEX_LIST_START_PATTERN))
                {
                    // Locate the start and end of the list block
                    

                    // Extract list template
                    List<XWPFParagraph> templateParagraphs = new List<XWPFParagraph>();
                    for (int j = startIdx + 1; j < endIdx; j++)
                    {
                        templateParagraphs.Add(document.Paragraphs[j]);
                    }

                    // Remove placeholders
                    document.RemoveBodyElement(startIdx); // Remove {{#Items}}
                    document.RemoveBodyElement(endIdx - 1); // Remove {{/Items}}

                    var items = (List<Dictionary<string, object>>)data["Items"];

                    // Insert processed list items
                    int insertPos = startIdx;
                    foreach (var item in items)
                    {
                        foreach (var templatePara in templateParagraphs)
                        {
                            XWPFParagraph newPara = doc.InsertNewParagraph(insertPos++);
                            newPara.Alignment = templatePara.Alignment;
                            newPara.SpacingAfter = templatePara.SpacingAfter;
                            newPara.SpacingBefore = templatePara.SpacingBefore;
                            newPara.SetText(stubble.Render(templatePara.Text, item));
                        }
                    }

                    break;
                }

                // Process single-line replacements
                para.ReplaceText(para.Text, stubble.Render(para.Text, data));
            }

            // Save output file
            using (FileStream outFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                doc.Write(outFile);
            }

            /*foreach (KeyValuePair<string, string> item in values)
            {
                document.FindAndReplaceText(item.Key, item.Value);
            }

            string newDocumentPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName(), Path.GetExtension(documentPath));

            using (var fs = new FileStream(newDocumentPath, FileMode.Create, FileAccess.Write))
            {
                document.Write(fs);
            }

            return newDocumentPath;*/

            throw new NotImplementedException();
        }
    }
}
