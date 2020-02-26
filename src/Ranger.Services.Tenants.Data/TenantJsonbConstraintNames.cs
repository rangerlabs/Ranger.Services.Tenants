namespace Ranger.Services.Tenants.Data
{
    internal class TenantJsonbConstraintNames
    {
        public const string Domain = "IX_tenant_unique_constraints_domain";
        public const string TenantId_Version = "idx_data_tenantid_version";
    }
}