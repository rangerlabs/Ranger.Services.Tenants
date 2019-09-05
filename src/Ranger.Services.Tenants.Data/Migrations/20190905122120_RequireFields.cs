using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Services.Tenants.Data
{
    public partial class RequireFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "registration_key",
                table: "tenants",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "organization_name",
                table: "tenants",
                maxLength: 28,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 28,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "domain",
                table: "tenants",
                maxLength: 28,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 28,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "database_username",
                table: "tenants",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "database_password",
                table: "tenants",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_database_username",
                table: "tenants",
                column: "database_username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tenants_database_username",
                table: "tenants");

            migrationBuilder.AlterColumn<string>(
                name: "registration_key",
                table: "tenants",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "organization_name",
                table: "tenants",
                maxLength: 28,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 28);

            migrationBuilder.AlterColumn<string>(
                name: "domain",
                table: "tenants",
                maxLength: 28,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 28);

            migrationBuilder.AlterColumn<string>(
                name: "database_username",
                table: "tenants",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "database_password",
                table: "tenants",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
