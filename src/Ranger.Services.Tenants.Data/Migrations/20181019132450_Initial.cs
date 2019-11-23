using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                       .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_on = table.Column<DateTime>(nullable: false),
                    last_accessed = table.Column<DateTime>(nullable: true),
                    name = table.Column<string>(maxLength: 28, nullable: true),
                    domain = table.Column<string>(maxLength: 28, nullable: true),
                    connection_string = table.Column<string>(nullable: true),
                    domain_password = table.Column<string>(nullable: true),
                    token = table.Column<string>(maxLength: 64, nullable: true),
                    domain_confirmed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_domain",
                table: "tenants",
                column: "domain",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}