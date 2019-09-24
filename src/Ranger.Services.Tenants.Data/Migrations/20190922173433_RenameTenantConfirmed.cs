using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Services.Tenants.Data
{
    public partial class RenameTenantConfirmed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "domain_confirmed",
                table: "tenants",
                newName: "enabled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "enabled",
                table: "tenants",
                newName: "domain_confirmed");
        }
    }
}
