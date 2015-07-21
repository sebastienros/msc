using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using WebAdvanced.Sitemap.Models;

namespace WebAdvanced.Sitemap.Providers.Impl {
    public class ContentRouteProvider : ISitemapRouteProvider {
        private readonly IRepository<SitemapSettingsRecord> _sitemapSettings;
        private readonly IContentManager _contentManager;
        private readonly UrlHelper _urlHelper;

        public ContentRouteProvider(
            IRepository<SitemapSettingsRecord> sitemapSettings,
            IContentManager contentManager,
            UrlHelper urlHelper) {
            _sitemapSettings = sitemapSettings;
            _contentManager = contentManager;
            _urlHelper = urlHelper;
        }

        public IEnumerable<SitemapRoute> GetDisplayRoutes() {
            // Get all active content types
            var types = _sitemapSettings
                .Fetch(q => q.IndexForDisplay)
                .ToDictionary(
                    k => k.ContentType,
                    v => v);

            if (types.Any()) {
                var contents = _contentManager.Query(VersionOptions.Published, types.Keys.ToArray()).List();
                return contents.Select(c => {
                    var metadata = _contentManager.GetItemMetadata(c);
                    return new SitemapRoute {
                        Title = metadata.DisplayText,
                        Url = _urlHelper.RouteUrl(metadata.DisplayRouteValues),
                        Priority = types[c.ContentType].Priority,
                        UpdateFrequency = types[c.ContentType].UpdateFrequency
                    };
                }).AsEnumerable();
            }

            return new List<SitemapRoute>();
        }

        public IEnumerable<SitemapRoute> GetXmlRoutes() {
            // Get all active content types
            var types = _sitemapSettings
                .Fetch(q => q.IndexForXml)
                .ToDictionary(
                    k => k.ContentType,
                    v => v);

            if (types.Any()) {
                var contents = _contentManager.Query(VersionOptions.Published, types.Keys.ToArray()).List();

                return contents.Select(c => {
                    var metadata = _contentManager.GetItemMetadata(c);
                    return new SitemapRoute {
                        Priority = types[c.ContentType].Priority,
                        Title = metadata.DisplayText,
                        UpdateFrequency = types[c.ContentType].UpdateFrequency,
                        Url = _urlHelper.RouteUrl(metadata.DisplayRouteValues),
                        LastUpdated = c.Has<CommonPart>() ? c.As<CommonPart>().ModifiedUtc : null
                    };
                }).AsEnumerable();
            }

            return new List<SitemapRoute>();
        }
        
        public int Priority { get { return 10; } }
    }
}
