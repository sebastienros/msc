using System;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Orchard.Blogs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;

namespace Msc.CustomFeedBuilder.Handlers {
    [OrchardFeature("Msc.CustomFeedBuilder.BlogFeedBuilder")]
    public class NotificationsHandler : ContentHandler {
        private ShellSettings _settings;
        public NotificationsHandler(ShellSettings settings) {
            OnPublished<BlogPostPart>(Update);
            OnUnpublished<BlogPostPart>(Update);
            OnRemoved<BlogPostPart>(Update);

            _settings = settings;
        }

        private void Update(ContentContextBase context, BlogPostPart blogPostPart) {
            try {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("Msc:StorageConnectionString"));

                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                // Retrieve a reference to a queue.
                CloudQueue queue = queueClient.GetQueueReference("updates");

                // Create the queue if it doesn't already exist
                queue.CreateIfNotExists();

                BlogPart blogPart = blogPostPart.BlogPart;

                if (blogPart == null) {
                    return;
                }

                var notification = new {
                    type = "BlogPost",
                    tenant = _settings.Name,
                    prefix = _settings.RequestUrlPrefix,
                    containerId = blogPart.Id
                };

                // Create a message and add it to the queue.
                CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(notification));

                queue.AddMessage(message);
            }
            catch (Exception e) {
                Logger.Error(e, "Unexpected error while queueing blog update");
            }
        }
    }
}