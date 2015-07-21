using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Syndication;
using Msc.CustomFeedBuilder.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Tasks;
using Orchard.Caching;
using System.Xml;
using Orchard.Services;
using System.Text;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Environment.Extensions;
using Orchard.Tags.Services;
using Orchard.Logging;
using Orchard.ContentManagement.Aspects;

namespace Msc.CustomFeedBuilder.Services {
    [OrchardFeature("Msc.BlogImport")]
    public class BlogImportBackgroundTask : IBackgroundTask {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IAutorouteService _autorouteService;
        private readonly ITagService _tagService;

        public BlogImportBackgroundTask(
            IContentManager contentManager, 
            IOrchardServices orchardServices,
            ICacheManager cacheManager,
            IClock clock,
            IAutorouteService autorouteService,
            ITagService tagService) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _cacheManager = cacheManager;
            _autorouteService = autorouteService;
            _clock = clock;
            _tagService = tagService;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {
            var blogImportSettings = _orchardServices.WorkContext.CurrentSite.As<BlogImportSettingsPart>();
            SyndicationFeed feed = _cacheManager.Get("BlogImport", ctx => {
                
                // Specify time for cache to expire
                ctx.Monitor(_clock.When(TimeSpan.FromSeconds(Math.Max(blogImportSettings.Delay, 60))));

                var blog = _contentManager.Query<BlogPart>("Blog").Slice(1).FirstOrDefault();
                if (blog == null) {
                    return null;
                }

                var lastBlogPost = _contentManager.Query<BlogPostPart>("BlogPost")
                    .OrderByDescending<CommonPartRecord>(x => x.CreatedUtc).Slice(1)
                    .FirstOrDefault();

                DateTime lastPostUtc = DateTime.MinValue;

                if (lastBlogPost != null) {
                    lastPostUtc = lastBlogPost.As<CommonPart>().CreatedUtc.Value;
                }

                SyndicationFeed result = new SyndicationFeed();

                try {
                    using (XmlReader reader = XmlReader.Create(blogImportSettings.Uri)) {
                        result = SyndicationFeed.Load(reader);
                    }

                    foreach (var item in result.Items) {
                        if (item.PublishDate.UtcDateTime <= lastPostUtc) {
                            continue;
                        }

                        try {
                            var itemAlias = item.Id.Split('/').Last();

                            if(String.IsNullOrWhiteSpace(itemAlias)) {
                                continue;
                            }
            
                            var existingPost = _contentManager.Query("BlogPost")
                                .Where<AutoroutePartRecord>(x => x.DisplayAlias == itemAlias).Slice(1)
                                .FirstOrDefault();

                            BlogPostPart newContentItem;
                            if (existingPost != null) {
                                newContentItem = existingPost.As<BlogPostPart>();
                            }
                            else {
                                newContentItem = _contentManager.New<BlogPostPart>("BlogPost");
                                newContentItem.BlogPart = blog;
                                _contentManager.Create(newContentItem);
                            }
                            
                            newContentItem.As<BodyPart>().Text = item.Summary.Text;
                            newContentItem.As<TitlePart>().Title = item.Title.Text;
                            newContentItem.As<CommonPart>().CreatedUtc = item.PublishDate.UtcDateTime;

                            newContentItem.Creator = blog.As<ICommonPart>().Owner;

                            if (item.Categories.Any()) {
                                _tagService.UpdateTagsForContentItem(newContentItem.ContentItem, item.Categories.Select(x => x.Name).ToArray());
                            }

                            // Update route part
                            newContentItem.As<AutoroutePart>().DisplayAlias = item.Id.Split('/').Last();
                            _autorouteService.PublishAlias(newContentItem.As<AutoroutePart>());

                            _contentManager.Publish(newContentItem.ContentItem);

                        }
                        catch(Exception e) {
                            Logger.Error(e, "Could not import post: " +  item.Id.ToString());
                            return null;
                        }
                    }
                }
                catch (Exception e) {
                    Logger.Error(e, "Could not load feed: " + blogImportSettings.Uri);
                    return null;
                }

                return result;

            });
        }
    }
}