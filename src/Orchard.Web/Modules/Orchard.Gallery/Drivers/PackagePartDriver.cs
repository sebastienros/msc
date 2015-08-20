using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Drivers {
    public class PackagePartDriver : ContentPartDriver<PackagePart> {
        private readonly IOrchardServices _orchardServices;

        public PackagePartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        protected override DriverResult Display(PackagePart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Package_Fields", () => shapeHelper.Parts_Package_Fields(Package: part)),
                ContentShape("Parts_Package_Fields_Summary", () => shapeHelper.Parts_Package_Fields_Summary(Package: part)),
                ContentShape("Parts_Package_Fields_SummaryAdmin", () => shapeHelper.Parts_Package_Fields_SummaryAdmin(Package: part))
            );
        }

        protected override DriverResult Editor(PackagePart part, dynamic shapeHelper) {
            return ContentShape("Parts_Package_Fields_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Package.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PackagePart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
