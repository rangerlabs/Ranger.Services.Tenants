using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ranger.Services.Tenants.Data {
    public interface ITenantRepository {
        Task<bool> ExistsAsync (string domain);
        bool Exists (string domain);
        Tenant FindTenantByDomain (string domain);
        Task<Tenant> FindTenantByDomainAsync (string domain);
        Task<Tenant> FindTenantByIDAsync (int id);
        Task<DatabaseCredentials> GetConnectionStringByTenantIdAsync (int id);
        Task<DatabaseCredentials> GetConnectionStringByDomainAsync (string domain);
        Task AddTenant (Tenant tenant);
        Task UpdateLastAccessed (string domain);
        Task Delete (Tenant tenant);
    }
}