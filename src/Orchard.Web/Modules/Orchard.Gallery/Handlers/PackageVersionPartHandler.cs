using System;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Handlers {
    public class PackageVersionPartHandler : ContentHandler {
        public PackageVersionPartHandler(IRepository<PackageVersionPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnUpdated<PackageVersionPart>((context, part) => {

                part.Record.NormalizedVersion = GetNormalizedVersion(part.Version);

                // Update package information
                var container = part.CommonPart.Container.As<PackagePart>();
                if(container != null) {
                    if (GetNormalizedVersion(container.LatestVersion) < GetNormalizedVersion(part.Version)) {
                        container.LatestVersionUtc = part.CommonPart.ModifiedUtc.Value;
                        container.LatestVersion = part.Version;
                    }
                }
            });

            OnIndexing<PackageVersionPart>((context, part) => {

                var container = part.CommonPart.Container.As<PackagePart>();

                if (container != null) {
                    context.DocumentIndex
                        .Add("package-extension-type", container.ExtensionType.ToString()).Store()
                        .Add("package-id", container.PackageId.ToLowerInvariant()).Analyze().Store()
                        ;

                    if (container != null && !String.IsNullOrWhiteSpace(container.TitlePart.Title)) {
                        context.DocumentIndex.Add(
                            "package-version-id",
                            container.PackageId.ToLowerInvariant() + "/" + part.Version
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

            if (!String.IsNullOrWhiteSpace(container.PackageId)) {
                context.Metadata.Identity.Add("package-version-id", container.PackageId.ToLowerInvariant() + "/" + part.Version);
            }

            context.Metadata.DisplayText = container.TitlePart.Title + " " + part.Version;

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Gallery"},
                {"Controller", "PackageVersion"},
                {"Action", "Display"},
                {"id", container.PackageId},
                {"version", part.Version}
            };
        }

        /// <summary>
        /// Creates a sortable version to order PackageVersion.
        /// </summary>
        private int GetNormalizedVersion(string version) {
            if(String.IsNullOrWhiteSpace(version)) {
                return 0;
            }

            var versionParts = version.Split('.');
            int normalizedVersion = Int32.Parse(versionParts[0]) * 1000000;

            if (versionParts.Length > 1) {
                normalizedVersion += Int32.Parse(versionParts[1]) * 1000;
            }

            if (versionParts.Length > 2) {
                normalizedVersion += Int32.Parse(versionParts[2]);
            }

            return normalizedVersion;
        }
    }
}
