using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Drivers {
    public class PackageVersionPartDriver : ContentPartDriver<PackageVersionPart> {
        private readonly IOrchardServices _orchardServices;

        public PackageVersionPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        protected override DriverResult Display(PackageVersionPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_PackageVersion_Fields", () => shapeHelper.Parts_PackageVersion_Fields(Package: part)),
                ContentShape("Parts_PackageVersion_Fields_Summary", () => shapeHelper.Parts_PackageVersion_Fields_Summary(Package: part)),
                ContentShape("Parts_PackageVersion_Fields_SummaryAdmin", () => shapeHelper.Parts_PackageVersion_Fields_SummaryAdmin(Package: part))
            );
        }

        protected override DriverResult Editor(PackageVersionPart part, dynamic shapeHelper) {
            return ContentShape("Parts_PackageVersion_Fields_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/PackageVersion.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PackageVersionPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(PackageVersionPart part, ExportContentContext context) {
            var partElement = context.Element(part.PartDefinition.Name);

            partElement.SetAttributeValue("DownloadCount", part.DownloadCount);
            partElement.SetAttributeValue("PackageUrl", part.PackageUrl);
            partElement.SetAttributeValue("Version", part.Version);
        }

        protected override void Importing(PackageVersionPart part, ImportContentContext context) {
            part.DownloadCount = Int32.Parse(context.Attribute(part.PartDefinition.Name, "DownloadCount"));
            part.PackageUrl = context.Attribute(part.PartDefinition.Name, "PackageUrl");
            part.Version = context.Attribute(part.PartDefinition.Name, "Version");
        }
    }
}
