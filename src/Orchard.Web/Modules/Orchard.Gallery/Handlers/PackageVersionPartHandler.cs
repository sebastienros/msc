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

            OnPublished<PackageVersionPart>(UpdateStorage);
            OnImported<PackageVersionPart>(UpdateStorage);
        }

        public void UpdateStorage(ContentContextBase context, PackageVersionPart part) {
            part.Record.NormalizedVersion = GetNormalizedVersion(part.Version);

            // Update package information
            var container = part.CommonPart.Container.As<PackagePart>();
            if (container != null) {
                part.Record.PackageVersionId = container.PackageId.ToLowerInvariant() + "/" + part.Version;

                if (GetNormalizedVersion(container.LatestVersion) < GetNormalizedVersion(part.Version)) {
                    container.LatestVersionUtc = part.CommonPart.ModifiedUtc.Value;
                    container.LatestVersion = part.Version;
                }
            }
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
        private long GetNormalizedVersion(string version) {
            if(String.IsNullOrWhiteSpace(version)) {
                return 0;
            }
            
            var versionParts = version.Split('.');
            long normalizedVersion = 0;

            if (versionParts.Length > 0) {
                normalizedVersion += Math.Min(999, Int64.Parse(versionParts[0])) * 1000000000;
            }

            if (versionParts.Length > 1) {
                normalizedVersion += Math.Min(999, Int64.Parse(versionParts[1])) * 1000000;
            }

            if (versionParts.Length > 2) {
                normalizedVersion += Math.Min(999, Int64.Parse(versionParts[2])) * 1000;
            }

            if (versionParts.Length > 3) {
                normalizedVersion += Int64.Parse(versionParts[3]);
            }

            return normalizedVersion;
        }
    }
}
