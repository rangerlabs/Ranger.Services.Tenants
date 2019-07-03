using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Services.Tenants.Data {
    public partial class RemoveDomainPass : Migration {
        protected override void Up (MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn (
                name: "domain_password",
                table: "tenants");
        }

        protected override void Down (MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string> (
                name: "domain_password",
                table: "tenants",
                nullable : true);
        }
    }
}