using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Services {
    public class PackageIdentityResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public PackageIdentityResolverSelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity) {
            if (contentIdentity.Has("package-id")) {
                return new IdentityResolverSelectorResult {
                    Priority = 0,
                    Resolve = ResolveIdentity
                };
            }

            return null;
        }

        private IEnumerable<ContentItem> ResolveIdentity(ContentIdentity identity) {
            var identifier = identity.Get("package-id");

            if (identifier == null) {
                return null;
            }

            return _contentManager
                .Query<PackagePart, PackagePartRecord>()
                .Where(p => p.PackageId == identifier)
                .List<ContentItem>();
        }
    }
}