using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.Serialization;
using Orchard.Caching;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Services;

namespace Msc.CustomFeedBuilder.Controllers {
    [OrchardFeature("Msc.CustomFeedBuilder.Tenants")]
    public class TenantsController : Controller {
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly ShellSettings _shellSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISessionLocator _sessionLocator;

        public TenantsController(
            IShellSettingsManager shellSettingsManager, 
            ShellSettings shellSettings,
            ICacheManager cacheManager,
            IClock clock,
            ISessionLocator sessionLocator) {
            _shellSettingsManager = shellSettingsManager;
            _shellSettings = shellSettings;
            _cacheManager = cacheManager;
            _clock = clock;
            _sessionLocator = sessionLocator;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ActionResult Index() {
            if (_shellSettings.Name != ShellSettings.DefaultName) {
                return HttpNotFound();
            }

            var tenants = _cacheManager.Get("tenants", ctx => {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(5)));

                return _shellSettingsManager
                    .LoadSettings()
                    .Where(x => x.Name != ShellSettings.DefaultName)
                    .Select(x => {
                        var tenant = new TenantResult {
                            Name = x.Name,
                            UrlHost = (x.RequestUrlHost ?? "").Trim(),
                            UrlPrefix = (x.RequestUrlPrefix ?? "").Trim(),
                            State = x.State.ToString(),
                            TablePrefix = (x.DataTablePrefix ?? "").Trim(),
                            BlogId = -1
                        };

                        try {
                            var connection = new SqlConnection(x.DataConnectionString);
                            using (connection) {
                                connection.Open();

                                var tablePrefix = String.IsNullOrWhiteSpace(tenant.TablePrefix) ? "" : tenant.TablePrefix + "_";
                                var sql = String.Format(@"
                                    SELECT cir.Id, cir.Data, civr.Data FROM  {0}Orchard_Framework_ContentTypeRecord AS ctr INNER JOIN  {0}Orchard_Framework_ContentItemRecord AS cir ON cir.ContentType_id = ctr.Id INNER JOIN {0}Orchard_Framework_ContentItemVersionRecord AS civr ON civr.ContentItemRecord_id = cir.Id WHERE ctr.Name = 'Blog' AND civr.Published = 1", tablePrefix);
                                try {
                                    var command = connection.CreateCommand();
                                    command.CommandText = sql;

                                    using (var reader = command.ExecuteReader()) {
                                        if (reader.Read()) {
                                            tenant.BlogId = reader.GetInt32(0);
                                            var data = XDocument.Parse(reader.GetString(1));
                                            var versiondata = XDocument.Parse(reader.GetString(2));
                                            tenant.Description = data.Root.Element("BlogPart").Attribute("Description").Value;
                                            tenant.Title = versiondata.Root.Element("TitlePart").Attribute("Title").Value;
                                        }
                                    }
                                }
                                catch (Exception e) {
                                    Logger.Error(e, "An error occured while requesting the list of tenants: " + sql);
                                }
                            }
                        }
                        catch (Exception e) {
                            Logger.Error(e, "An error occured while opening the connection: " + x.DataConnectionString + " for tenant: " + tenant.Name);
                        }

                        return tenant;
                    }).ToList();

            });

            var ser = new XmlSerializer(typeof (List<TenantResult>));
            using (var sw = new StringWriter()) {
                ser.Serialize(sw, tenants);

                return Content(sw.ToString(), "text/xml");
            }
        }
    }

    public class TenantResult {
        public string Name { get; set; }
        public string UrlHost { get; set; }
        public string UrlPrefix { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public int BlogId { get; set; }
        internal string TablePrefix { get; set; }

    }
}