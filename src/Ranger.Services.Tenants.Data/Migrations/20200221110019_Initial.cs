using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ranger.Services.Tenants.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(nullable: true),
                    xml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_streams",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    stream_id = table.Column<Guid>(nullable: false),
                    version = table.Column<int>(nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    @event = table.Column<string>(name: "event", nullable: false),
                    inserted_at = table.Column<DateTime>(nullable: false),
                    inserted_by = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_streams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_unique_constraints",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(nullable: false),
                    domain = table.Column<string>(maxLength: 28, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_unique_constraints", x => x.tenant_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_unique_constraints_domain",
                table: "tenant_unique_constraints",
                column: "domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_unique_constraints_tenant_id",
                table: "tenant_unique_constraints",
                column: "tenant_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "tenant_streams");

            migrationBuilder.DropTable(
                name: "tenant_unique_constraints");
        }
    }
}
