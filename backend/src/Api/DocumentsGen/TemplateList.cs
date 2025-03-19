using NPOI.SS.UserModel;

namespace Fei.Is.Api.DocumentsGen
{
    public class TemplateList
    {
        public TemplateRow startRow { get; set; }
        public TemplateRow endRow { get; set; }
        public string tag { get; set; }

        public List<TemplateRow> insertedRows = new List<TemplateRow>();
    }
}
