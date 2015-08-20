using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Gallery.Utils;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Search.Services;

namespace Orchard.Gallery.Controllers {
    public class PackageVersionController : Controller {
        private readonly ISearchService _searchService;
        private readonly IIndexManager _indexManager;
        private readonly IOrchardServices _orchardService;

        public PackageVersionController(
            IOrchardServices orchardService,
            ISearchService searchService,
            IIndexManager indexManager
            ) {
            _searchService = searchService;
            _orchardService = orchardService;
            _indexManager = indexManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Display(string id, string version) {

            if (String.IsNullOrWhiteSpace(id) || String.IsNullOrWhiteSpace(version)) {
                return HttpNotFound();
            }

            var searchBuilder = GetSearchBuilder();

            var document = searchBuilder
                .WithField("type", "PackageVersion").ExactMatch()
                .WithField("package-version-id", id.ToLowerInvariant() + "/" + version).ExactMatch()
                .Slice(0, 1)
                .Search()
                .FirstOrDefault();

            if (document == null) {
                return HttpNotFound();
            }

            return new TransferToRouteResult(new RouteValueDictionary {
                { "action", "Display" },
                { "controller", "Item" },
                { "area", "Contents" },
                { "id", document.GetInt("id") }
            });
        }

        ISearchBuilder GetSearchBuilder() {
            return _indexManager
                .GetSearchIndexProvider()
                .CreateSearchBuilder("PackageVersions")
                ;
        }
    }


}