using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Gallery {
    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("PackagePartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("PackageId", c => c.WithLength(255).Unique())
                );

            SchemaBuilder.CreateTable("PackageVersionPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("NormalizedVersion", c => c.WithDefault(0))
                );

            return 1;
        }
    }
}
