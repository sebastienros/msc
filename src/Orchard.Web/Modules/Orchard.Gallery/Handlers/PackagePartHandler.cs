using System;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Handlers {
    public class PackagePartHandler : ContentHandler {
        public PackagePartHandler() {

            OnIndexing<PackagePart>((context, packagePart) => {

                context.DocumentIndex
                    .Add("package-download-count", packagePart.DownloadCount).Store()
                    .Add("package-extension-type", packagePart.ExtensionType.ToString().ToLowerInvariant()).Store()
                    .Add("package-id", packagePart.TitlePart.Title.ToLowerInvariant()).Store()
                ;
            });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var packagePart = context.ContentItem.As<PackagePart>();

            if (packagePart == null)
                return;

            if (!String.IsNullOrWhiteSpace(packagePart.TitlePart.Title)) {
                context.Metadata.Identity.Add("package-id", packagePart.TitlePart.Title);
            }

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Gallery"},
                {"Controller", "Package"},
                {"Action", "Display"},
                {"id", packagePart.TitlePart.Title}
            };
        }
    }
}
