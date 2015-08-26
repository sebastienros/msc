using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Gallery.Models;
using Orchard.Gallery.Utils;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace Orchard.Gallery.Controllers {
    [Themed]
    public class PackageController : Controller {
        private readonly IIndexManager _indexManager;
        private readonly IOrchardServices _orchardService;

        public PackageController(
            IOrchardServices orchardService,
            IIndexManager indexManager
            ) {
            _orchardService = orchardService;
            _indexManager = indexManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        public ActionResult Create(string title, string tags, string description, string copyright, string projectUrl, string licenseUrl) {

            return View();
        }

        public ActionResult Edit(int id) {

            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, string title, string tags, string description, string copyright, string projectUrl, string licenseUrl) {

            return View();
        }

        public ActionResult Display(string id) {
            if(String.IsNullOrWhiteSpace(id)) {
                return HttpNotFound();
            }

            var package = _orchardService.ContentManager
                .Query<PackagePart, PackagePartRecord>()
                .Where(p => p.PackageId == id)
                .List()
                .FirstOrDefault();

            if (package == null) {
                return HttpNotFound();
            }

            return new TransferToRouteResult(new RouteValueDictionary {
                { "action", "Display" },
                { "controller", "Item" },
                { "area", "Containers" },
                { "id", package.Id }
            });
        }

        public ActionResult Index(PagerParameters pagerParameters, string type = "Module", string q = "") {

            var pager = new Pager(_orchardService.WorkContext.CurrentSite, pagerParameters);

            var searchBuilder = GetSearchBuilder();

            if(!String.IsNullOrWhiteSpace(q)) {
                searchBuilder.Parse(
                    defaultFields: new[] { "body", "title", "tags", "package-id" }, 
                    query: q, 
                    escape: true
                );
            }

            searchBuilder.WithField("package-extension-type", type.ToLowerInvariant()).NotAnalyzed().ExactMatch();
            searchBuilder.SortByInteger("package-download-count");

            var count = searchBuilder.Count();
            var pageOfResults = searchBuilder.Slice((pager.Page - 1) * pager.PageSize + 1, pager.PageSize).Search();

            var list = _orchardService.New.List();
            var foundIds = pageOfResults.Select(searchHit => searchHit.ContentItemId).ToList();

            var foundItems = _orchardService.ContentManager.GetMany<IContent>(foundIds, VersionOptions.Published, new QueryHints()).ToList();
            foreach (var contentItem in foundItems) {
                list.Add(_orchardService.ContentManager.BuildDisplay(contentItem, "Summary"));
            }

            var pagerShape = _orchardService.New.Pager(pager).TotalItemCount(count);

            var searchViewModel = _orchardService.New.ViewModel(
                Query: q,
                TotalItemCount: count,
                StartPosition: (pager.Page - 1) * pager.PageSize + 1,
                EndPosition: pager.Page * pager.PageSize > count ? count : pager.Page * pager.PageSize,
                ContentItems: list,
                Pager: pagerShape,
                Type: type
            );

            return View(searchViewModel);
        }

        ISearchBuilder GetSearchBuilder() {
            return _indexManager
                .GetSearchIndexProvider()
                .CreateSearchBuilder("Packages")
                ;
        }

    }
}