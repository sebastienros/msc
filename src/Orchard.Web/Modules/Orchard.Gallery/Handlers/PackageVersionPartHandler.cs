using System;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Handlers {
    public class PackageVersionPartHandler : ContentHandler {
        public PackageVersionPartHandler() {

            OnIndexing<PackageVersionPart>((context, part) => {

                var container = part.CommonPart.Container.As<PackagePart>();

                if (container != null) {
                    context.DocumentIndex
                        .Add("package-extension-type", container.ExtensionType.ToString()).Store()
                        .Add("package-id", container.TitlePart.Title.ToLowerInvariant()).Store()
                        ;

                    if (container != null && !String.IsNullOrWhiteSpace(container.TitlePart.Title)) {
                        context.DocumentIndex.Add(
                            "package-version-id",
                            container.TitlePart.Title.ToLowerInvariant() + "/" + part.Version
                        ).Store();
                    }
                }

            });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<PackageVersionPart>();

            if (part == null)
                return;

            var container = part.CommonPart.Container.As<PackagePart>();

            if (container == null)
                return;

            if (!String.IsNullOrWhiteSpace(container.TitlePart.Title)) {
                context.Metadata.Identity.Add("package-version-id", container.TitlePart.Title.ToLowerInvariant() + "/" + part.Version);
            }

            context.Metadata.DisplayText = container.TitlePart.Title + " " + part.Version;

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Gallery"},
                {"Controller", "PackageVersion"},
                {"Action", "Display"},
                {"id", container.TitlePart.Title},
                {"version", part.Version}
            };
        }
    }
}
