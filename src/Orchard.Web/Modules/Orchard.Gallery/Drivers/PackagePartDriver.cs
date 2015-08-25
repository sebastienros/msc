using System;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Gallery.Models;
using Orchard.Localization;

namespace Orchard.Gallery.Drivers {
    public class PackagePartDriver : ContentPartDriver<PackagePart> {
        private readonly IOrchardServices _orchardServices;

        public PackagePartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(PackagePart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Package_Fields", () => shapeHelper.Parts_Package_Fields(Package: part)),
                ContentShape("Parts_Package_Fields_Summary", () => shapeHelper.Parts_Package_Fields_Summary(Package: part)),
                ContentShape("Parts_Package_Fields_SummaryAdmin", () => shapeHelper.Parts_Package_Fields_SummaryAdmin(Package: part)),
                ContentShape("Parts_Package_PackageVersions", () => {
                    var versions = _orchardServices.ContentManager
                        .Query<PackageVersionPart, PackageVersionPartRecord>()
                        .Where<CommonPartRecord>(x => x.Container.Id == part.Id)
                        .OrderByDescending<PackageVersionPartRecord>(x => x.NormalizedVersion)
                        .List()
                        .ToList();

                    return shapeHelper.Parts_Package_PackageVersions(Package: part, PackageVersions: versions);
                })
            );
        }

        protected override DriverResult Editor(PackagePart part, dynamic shapeHelper) {
            return ContentShape("Parts_Package_Fields_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Package.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PackagePart part, IUpdateModel updater, dynamic shapeHelper) {

            updater.TryUpdateModel(part, Prefix, null, null);

            // Ensure the package id is unique and valid.

            if (!Regex.IsMatch(part.PackageId, @"^[A-Za-z0-9][A-Za-z0-9\.]+$")) {
                updater.AddModelError("PackageId", T("The package id can only contain alpha-numeric characters and dots."));
            }

            // Ensure a package with the same title doesn't already exist.
            else {
                var existingPackage = _orchardServices.ContentManager
                    .Query<PackagePart, PackagePartRecord>(VersionOptions.Published)
                    .Where(x => x.PackageId == part.PackageId && x.Id != part.Id)
                    .Slice(0, 1)
                    .FirstOrDefault();

                if (existingPackage != null) {
                    updater.AddModelError("title", T("A package with the same package id already exists."));
                }
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(PackagePart part, ExportContentContext context) {
            var partElement = context.Element(part.PartDefinition.Name);

            partElement.SetAttributeValue("DownloadCount", part.DownloadCount);
            partElement.SetAttributeValue("ExtensionType", part.ExtensionType);
            partElement.SetAttributeValue("IconUrl", part.Retrieve<string>("IconUrl"));
            partElement.SetAttributeValue("License", part.License);
            partElement.SetAttributeValue("LicenseUrl", part.LicenseUrl);
            partElement.SetAttributeValue("PackageId", part.PackageId);
            partElement.SetAttributeValue("ProjectUrl", part.ProjectUrl);
            partElement.SetAttributeValue("ScreenshotUrls", part.Retrieve<string>("ScreenshotUrls"));
            partElement.SetAttributeValue("Summary", part.Summary);
        }

        protected override void Importing(PackagePart part, ImportContentContext context) {
            part.DownloadCount = Int32.Parse(context.Attribute(part.PartDefinition.Name, "DownloadCount"));
            part.ExtensionType = (PackagePart.ExtensionTypes)Enum.Parse(typeof(PackagePart.ExtensionTypes), context.Attribute(part.PartDefinition.Name, "ExtensionType"), true);
            part.Store("IconUrl", context.Attribute(part.PartDefinition.Name, "IconUrl"));
            part.License = context.Attribute(part.PartDefinition.Name, "License");
            part.LicenseUrl = context.Attribute(part.PartDefinition.Name, "LicenseUrl");
            part.PackageId = context.Attribute(part.PartDefinition.Name, "PackageId");
            part.ProjectUrl = context.Attribute(part.PartDefinition.Name, "ProjectUrl");
            part.Store("ScreenshotUrls", context.Attribute(part.PartDefinition.Name, "ScreenshotUrls"));
            part.Summary = context.Attribute(part.PartDefinition.Name, "Summary");
        }
    }
}
