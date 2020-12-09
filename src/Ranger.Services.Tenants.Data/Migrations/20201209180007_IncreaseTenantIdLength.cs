﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Services.Tenants.Data.Migrations
{
    public partial class IncreaseTenantIdLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tenant_id",
                table: "tenant_unique_constraints",
                maxLength: 41,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tenant_id",
                table: "tenant_unique_constraints",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 41);
        }
    }
}
