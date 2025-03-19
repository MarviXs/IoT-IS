using NPOI.SS.UserModel;

namespace Fei.Is.Api.DocumentsGen
{
    public class TemplateRow
    {
        public IRow Row { get; set; }
        public bool deleted { get; set; }

        public TemplateRow(IRow row)
        {
            Row = row;
            deleted = false;
        }
    }
}
