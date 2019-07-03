using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ranger.Services.Tenants.Data
{
    public partial class UpdateTenantModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "tenants",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "connection_string",
                table: "tenants",
                newName: "database_username");

            migrationBuilder.RenameIndex(
                name: "IX_tenants_domain",
                table: "tenants",
                newName: "ix_tenants_domain");

            migrationBuilder.AddColumn<string>(
                name: "database_password",
                table: "tenants",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    friendly_name = table.Column<string>(nullable: true),
                    xml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropColumn(
                name: "database_password",
                table: "tenants");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                table: "tenants",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "database_username",
                table: "tenants",
                newName: "connection_string");

            migrationBuilder.RenameIndex(
                name: "ix_tenants_domain",
                table: "tenants",
                newName: "IX_tenants_domain");
        }
    }
}
