using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Services.Tenants.Data.Migrations
{
    public partial class AddJsonbIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //https://www.postgresql.org/docs/9.6/datatype-json.html
            //https://dba.stackexchange.com/questions/161313/creating-a-unique-constraint-from-a-json-object
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_data_tenantid_version ON tenant_streams ((data->>'TenantId'), version);");
            migrationBuilder.Sql("CREATE INDEX idx_data_databaseusername ON tenant_streams ((data ->> 'DatabaseUsername'));");
            migrationBuilder.Sql("CREATE INDEX idx_data_domain ON tenant_streams ((data ->> 'Domain'));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX idx_data_tenantid_version");
            migrationBuilder.Sql("DROP INDEX idx_data_databaseusername");
            migrationBuilder.Sql("DROP INDEX idx_data_domain");
        }
    }
}
