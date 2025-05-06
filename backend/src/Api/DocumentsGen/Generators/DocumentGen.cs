using Newtonsoft.Json.Linq;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Core.Settings;

namespace Fei.Is.Api.DocumentsGen.Generators
{
    public abstract class DocumentGen
    {
        protected const string REGEX_LIST_ITEM_PATTERN = "{{(.*?)}}";
        protected const string REGEX_ITEM_PATTERN = "{{([^./#][^}]*)}}";
        protected const string REGEX_LIST_START_PATTERN = "{{#(.*?)}}";
        protected const string REGEX_LIST_START_FORMAT = "{{#%s}}";
        protected const string REGEX_LIST_END_PATTERN = "{{/(.*?)}}";
        protected const string REGEX_LIST_END_FORMAT = "{{/%s}}";
        protected const string REGEX_IMAGE_PATTERN = "{{image:(.*?)}}";
        protected StubbleVisitorRenderer StubbleRenderer = new StubbleBuilder()
            .Configure(settings => settings.SetEncodingFunction(s => s))
            .Build();
        public abstract string ApplyFields(FileStream file, string newFileName, JToken values);
    }
}
