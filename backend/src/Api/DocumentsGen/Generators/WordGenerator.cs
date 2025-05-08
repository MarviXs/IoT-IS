using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
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
        public override string ApplyFields(FileStream fileStream, string newFileName, JToken values, Dictionary<string, string> images)
        {
            throw new NotImplementedException();
        }
    }
}
