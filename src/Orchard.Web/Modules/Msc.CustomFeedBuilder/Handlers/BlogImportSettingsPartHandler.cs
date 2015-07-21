using System.Linq;
using Msc.CustomFeedBuilder.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace GitContent.Handlers {
    [OrchardFeature("Msc.BlogImport")]
    public class BlogImportSettingsPartHandler : ContentHandler {

        public BlogImportSettingsPartHandler()
        {
            Filters.Add(new ActivatingFilter<BlogImportSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<BlogImportSettingsPart>("BlogImportSettings", "Parts/BlogImportSiteSettings", "blogimport"));
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("BlogImport")));
        }
    }
}