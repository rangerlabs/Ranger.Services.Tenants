using System.Threading.Tasks;

namespace Ranger.Services.Tenants.Data
{
    public interface ITenantsRepository
    {
        Task AddTenant(string userEmail, Tenant tenant);
        Task<bool> ExistsAsync(string domain);
        Task<Tenant> FindTenantByDatabaseUsernameAsync(string databaseUsername);
        Task<Tenant> FindTenantByDomainAsync(string domain);
        Task SoftDelete(string userEmail, string domain);
        Task UpdateLastAccessed(string domain);
        Task<Tenant> UpdateTenantAsync(string userEmail, string eventName, int version, Tenant tenant);
    }
}