using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public interface ITenantsRepository
    {

        Task CompletePrimaryOwnerTransfer(string userEmail, string domain, PrimaryOwnerTransferStateEnum state);
        Task AddPrimaryOwnerTransfer(string userEmail, string domain, PrimaryOwnerTransfer transfer);
        Task AddTenant(string userEmail, Tenant tenant);
        Task<bool> ExistsAsync(string domain);
        Task<Tenant> FindTenantByDatabaseUsernameAsync(string databaseUsername);
        Task<Tenant> FindTenantByDomainAsync(string domain);
        Task SoftDelete(string userEmail, string domain);
        Task UpdateLastAccessed(string domain);
        Task UpdateTenantAsync(string userEmail, string eventName, int version, Tenant tenant);
    }
}