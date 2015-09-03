using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Core.Feeds.Models;
using Orchard.Services;
using Orchard.Utility.Extensions;
using Orchard.Blogs.Models;
using Orchard.Environment.Extensions;
using Orchard;

namespace Msc.CustomFeedBuilder
{
    [OrchardFeature("Msc.CustomFeedBuilder.BlogFeedBuilder")]
    [UsedImplicitly]
    public class BlogFeedBuilder : IFeedItemBuilder {
        private readonly IContentManager _contentManager;
        private readonly RouteCollection _routes;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly IOrchardServices _orchardServices;

        public BlogFeedBuilder(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            RouteCollection routes,
            IEnumerable<IHtmlFilter> htmlFilters) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _routes = routes;
            _htmlFilters = htmlFilters;
        }

        public void Populate(FeedContext context) {
            if (context.ValueProvider.GetValue("blogaggregation") != null) {

                var baseUrl = new Uri(new Uri(_orchardServices.WorkContext.CurrentSite.BaseUrl), "rssnamespace");
                var uriBuilder = new UriBuilder(baseUrl.ToString());
                uriBuilder.Scheme = "http";
                XNamespace ns = uriBuilder.ToString();

                string description;

                foreach (var feedItem in context.Response.Items.OfType<FeedItem<ContentItem>>()) {
                    var inspector = new ItemInspector(
                        feedItem.Item,
                        _contentManager.GetItemMetadata(feedItem.Item),
                        _htmlFilters);

                    // TODO: author


                    // add to known formats
                    if (context.Format == "rss") {
                        //string xmlns = ""
                        //var link = new XElement("link");
                        //var guid = new XElement("guid", new XAttribute("isPermaLink", "true"));

                        //feedItem.Element.SetElementValue("title", inspector.Title);
                        //feedItem.Element.Add(link);
                        //feedItem.Element.SetElementValue("description", inspector.Description);

                        var parentBlog = feedItem.Item.As<BlogPart>();
                        if (parentBlog != null) {
                            //this is not a blog post, don't include
                            feedItem.Element.RemoveAll();
                            continue;
                        }


                        var blogPost = feedItem.Item.As<BlogPostPart>();
                        var blogTitle = new XElement(ns + "title");
                        var blogAuthor = new XElement(ns + "author");
                        var blogDescription = new XElement(ns + "description");
                        if (blogPost != null) {
                            //add blog description
                            blogDescription.Add(blogPost.BlogPart.Description);

                            //add blog title                            
                            blogTitle.Add(blogPost.BlogPart.Name);

                            //add blog author
                            if (blogPost.Creator != null) {
                                blogAuthor.Add(blogPost.Creator.UserName);
                            }
                        }
                        var blog = new XElement("blog",
                            new XAttribute(XNamespace.Xmlns + "msc", ns));
                        blog.Add(blogTitle);
                        blog.Add(blogDescription);
                        blog.Add(blogAuthor);

                        feedItem.Element.Add(blog);
                        //feedItem.Element.Add(blogTitle);
                        //feedItem.Element.Add(blogAuthor);

                        ////add blogtitle                        
                        //context.Response.Contextualize(requestContext =>
                        //{
                        //    var urlHelper = new UrlHelper(requestContext);
                        //    var uriBuilder = new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString()) { Path = urlHelper.RouteUrl(inspector.Link) };

                        //    var blogTitle = new XElement("blogtitle", new XAttribute("isPermaLink", urlHelper.RouteUrl(inspector.Link)));
                        //    blogTitle.Add(parentBlog.Name);
                        //    feedItem.Element.Add(blogTitle);
                        //});

                        //clean up description
                        description = feedItem.Element.Element("description").Value == null ? "" : feedItem.Element.Element("description").Value.RemoveTags();
                        if (description.Length > 300) {
                            description = description.Substring(0, 300);
                        }
                        feedItem.Element.Element("description").Value = description;

                        //if (inspector.PublishedUtc != null)
                        //{
                        //    // RFC833 
                        //    // The "R" or "r" standard format specifier represents a custom date and time format string that is defined by 
                        //    // the DateTimeFormatInfo.RFC1123Pattern property. The pattern reflects a defined standard, and the property  
                        //    // is read-only. Therefore, it is always the same, regardless of the culture used or the format provider supplied.  
                        //    // The custom format string is "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'". When this standard format specifier is used,  
                        //    // the formatting or parsing operation always uses the invariant culture. 
                        //    feedItem.Element.SetElementValue("pubDate", inspector.PublishedUtc.Value.ToString("r"));
                        //}

                        //feedItem.Element.Add(guid);
                    }
                }
            }
        }
    }
}