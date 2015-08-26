using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Services {
    public class PackageVersionIdentityResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public PackageVersionIdentityResolverSelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity) {
            if (contentIdentity.Has("package-version-id")) {
                return new IdentityResolverSelectorResult {
                    Priority = 0,
                    Resolve = ResolveIdentity
                };
            }

            return null;
        }

        private IEnumerable<ContentItem> ResolveIdentity(ContentIdentity identity) {
            var identifier = identity.Get("package-version-id");

            if (identifier == null) {
                return null;
            }

            return _contentManager
                .Query<PackageVersionPart, PackageVersionPartRecord>()
                .Where(p => p.PackageVersionId == identifier)
                .List<ContentItem>();
        }
    }
}