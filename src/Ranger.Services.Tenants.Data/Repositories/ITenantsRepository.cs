using System.Collections.Generic;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public interface ITenantsRepository
    {

        Task CompletePrimaryOwnerTransferAsync(string userEmail, string domain, PrimaryOwnerTransferStateEnum state);
        Task AddPrimaryOwnerTransferAsync(string userEmail, string domain, PrimaryOwnerTransfer transfer);
        Task AddTenant(string userEmail, Tenant tenant);
        Task<bool> ExistsAsync(string domain);
        Task<(Tenant tenant, int version)> FindNotDeletedTenantByTenantIdAsync(string tenantId);
        Task<(Tenant tenant, int version)> FindNotDeletedTenantByDomainAsync(string domain);
        Task<bool> IsTenantConfirmedAsync(string domain);
        Task<string> SoftDelete(string userEmail, string tenantId);
        Task UpdateLastAccessed(string domain);
        Task UpdateTenantAsync(string userEmail, string eventName, int version, Tenant tenant);
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();
    }
}