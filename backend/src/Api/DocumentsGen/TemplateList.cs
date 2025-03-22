using NPOI.SS.UserModel;

namespace Fei.Is.Api.DocumentsGen
{
    public class TemplateList
    {
        public IRow? startRow { get; set; }
        public IRow? endRow { get; set; }
        public string tag { get; set; }
    }
}
