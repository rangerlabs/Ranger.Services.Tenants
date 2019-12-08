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
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_on = table.Column<DateTime>(nullable: false),
                    last_accessed = table.Column<DateTime>(nullable: true),
                    organization_name = table.Column<string>(maxLength: 28, nullable: false),
                    domain = table.Column<string>(maxLength: 28, nullable: false),
                    database_username = table.Column<string>(nullable: false),
                    database_password = table.Column<string>(nullable: false),
                    token = table.Column<string>(maxLength: 64, nullable: false),
                    enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_database_username",
                table: "tenants",
                column: "database_username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_domain",
                table: "tenants",
                column: "domain",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
