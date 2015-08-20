using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System.Linq;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using System.Text.RegularExpressions;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Drivers {
    public class TitlePartDriver : ContentPartDriver<TitlePart> {
        private readonly IOrchardServices _orchardServices;

        public TitlePartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(TitlePart part, IUpdateModel updater, dynamic shapeHelper) {

            // If the content item is a package, ensure the title is unique and a valid package id.

            if (part.As<PackagePart>() != null) {

                part.Title = (part.Title ?? "").Trim();

                // Ensure the title is valid.
                if (!Regex.IsMatch(part.Title, @"^[A-Za-z0-9][A-Za-z0-9\.]+$")) {
                    updater.AddModelError("title", T("The title can only contain alpha-numeric characters and dots."));
                }

                // Ensure a package with the same title doesn't already exist.
                else {
                    var otherPackages = _orchardServices.ContentManager
                        .Query<TitlePart, TitlePartRecord>()
                        .Where(x => x.Title == part.Title)
                        .Slice(0, 1)
                        .FirstOrDefault();

                    if (otherPackages != null && otherPackages.Id != part.Id) {
                        updater.AddModelError("title", T("A package with the same title already exists."));
                    }
                }
            }

            return Editor(part, shapeHelper);
        }
    }
}
