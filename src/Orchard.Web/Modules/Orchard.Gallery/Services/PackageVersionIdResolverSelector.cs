using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace Orchard.Autoroute.Services {
    public class PackageVersionIdResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public PackageVersionIdResolverSelector(IContentManager contentManager) {
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
                .Query<TitlePart, TitlePartRecord>()
                .Where(p => p.Title == identifier)
                .List<ContentItem>();
        }
    }
}