namespace Ranger.Services.Tenants
{
    public static class RedisKeys
    {
        public static string GetTenantId(string tenantId) => $"GET_TENANT_ID_{tenantId}";
        public static string GetTenantDomain(string tenantId) => $"GET_TENANT_DOMAIN_{tenantId}";
    }
}