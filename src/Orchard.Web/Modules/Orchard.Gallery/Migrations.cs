using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;

namespace Orchard.Gallery {
    public class Migrations : DataMigrationImpl {
        private readonly IOrchardServices _services;

        public Migrations(IOrchardServices services) {
            _services = services;
        }

        public int Create() {

            ContentDefinitionManager.AlterTypeDefinition("Package", cfg => cfg
                .WithPart("PackagePart")
                .WithPart("CommonPart", p => p
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "true")
                    .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                .WithPart("TitlePart")
                .WithPart("IdentityPart")
                .WithPart("TagsPart")
                .WithPart("BodyPart")
                .WithPart("ContainerPart")

                .Creatable()
                .Listable()
            );

            ContentDefinitionManager.AlterTypeDefinition("PackageVersion", cfg => cfg
                .WithPart("PackageVersionPart")
                .WithPart("CommonPart")
                .WithPart("ContainablePart")

                .WithSetting("ContainableTypePartSettings.ShowContainerPicker", "false")
                .WithSetting("ContainableTypePartSettings.ShowPositionEditor", "false")

                .Creatable()
                .Listable()
            );

            return 1;
        }
    }
}
