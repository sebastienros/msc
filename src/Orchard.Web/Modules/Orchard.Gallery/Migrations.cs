using Orchard.Data.Migration;

namespace Orchard.Gallery {
    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("PackagePartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("PackageId", c => c.WithLength(1024).Unique())
                );

            SchemaBuilder.CreateTable("PackageVersionPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<long>("NormalizedVersion", c => c.WithDefault(0))
                    .Column<string>("PackageVersionId", c => c.WithLength(1024).Unique())
                );

            return 1;
        }
    }
}
