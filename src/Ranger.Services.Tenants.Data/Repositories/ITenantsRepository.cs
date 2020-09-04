using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public interface ITenantsRepository
    {

        Task<string> CompletePrimaryOwnerTransferAsync(string userEmail, string domain, PrimaryOwnerTransferStateEnum state);
        Task<string> AddPrimaryOwnerTransferAsync(string userEmail, string domain, PrimaryOwnerTransfer transfer);
        Task AddTenant(string userEmail, Tenant tenant);
        Task<bool> ExistsAsync(string domain, CancellationToken cancellationToken = default(CancellationToken));
        Task<(Tenant tenant, int version)> GetNotDeletedTenantByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default(CancellationToken));
        Task<(Tenant tenant, int version)> GetNotDeletedTenantByDomainAsync(string domain, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> IsTenantConfirmedAsync(string domain, CancellationToken cancellationToken = default(CancellationToken));
        Task<(string orgName, string domain)> SoftDelete(string userEmail, string tenantId);
        Task UpdateLastAccessed(string domain);
        Task UpdateTenantAsync(string userEmail, string eventName, int version, Tenant tenant);
        Task<IEnumerable<Tenant>> GetAllNotDeletedAndConfirmedTenantsAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}