using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace NGM.OpenAuthentication {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("UserProviderRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("UserId")
                    .Column<string>("ProviderName")
                    .Column<string>("ProviderUserId")
                );

            SchemaBuilder.AlterTable("UserProviderRecord", table => table
                .CreateIndex("IDX_UserProviderRecord_UserId_ProviderName_ProviderUserId", "UserId", "ProviderName", "ProviderUserId")
            );

            SchemaBuilder.CreateTable("ProviderConfigurationRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("IsEnabled")
                    .Column<string>("DisplayName")
                    .Column<string>("ProviderName")
                    .Column<string>("ProviderIdKey")
                    .Column<string>("ProviderSecret")
                    .Column<string>("ProviderIdentifier")
                );

            SchemaBuilder.AlterTable("ProviderConfigurationRecord", table => table
                .CreateIndex("IDX_ProviderConfigurationRecord_ProviderName", "ProviderName")
            );

            return 9;
        }

        public int UpdateFrom8() {
            SchemaBuilder.DropTable("OpenAuthenticationSettingsPartRecord");

            SchemaBuilder.AlterTable("UserProviderRecord", table => table
                .CreateIndex("IDX_UserProviderRecord_UserId_ProviderName_ProviderUserId", "UserId",  "ProviderName", "ProviderUserId")
            );

            SchemaBuilder.AlterTable("ProviderConfigurationRecord", table => table
                .CreateIndex("IDX_ProviderConfigurationRecord_ProviderName", "ProviderName")
            );

            return 9;
        }
    }
}