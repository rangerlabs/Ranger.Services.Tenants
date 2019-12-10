using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ranger.Services.Tenants.Data
{
    public class TenantRepository : ITenantRepository
    {
        private readonly TenantDbContext context;

        public TenantRepository(TenantDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> ExistsAsync(string domain)
        {
            return await context.Tenants.AnyAsync((t => t.Domain == domain.ToLowerInvariant()));
        }

        public async Task<Tenant> FindTenantEnabledByDatabaseUsernameAsync(string databaseUsername)
        {
            return await context.Tenants.SingleOrDefaultAsync(_ => _.DatabaseUsername == databaseUsername);
        }

        public bool Exists(string domain)
        {
            return context.Tenants.Any((t => t.Domain == domain.ToLowerInvariant()));
        }

        public Tenant FindTenantByDomain(string domain)
        {
            return context.Tenants.SingleOrDefault((t => t.Domain == domain.ToLowerInvariant()));
        }

        public async Task<Tenant> FindTenantByDomainAsync(string domain)
        {
            return await context.Tenants.SingleOrDefaultAsync((t => t.Domain == domain.ToLowerInvariant()));
        }

        public async Task<Tenant> FindTenantByIDAsync(int id)
        {
            return await context.Tenants.FindAsync(id);
        }

        public async Task<DatabaseCredentials> GetConnectionStringByTenantIdAsync(int id)
        {
            var result = await context.Tenants.FindAsync(id);
            return new DatabaseCredentials(result.DatabaseUsername, result.DatabasePassword);
        }

        public async Task<DatabaseCredentials> GetConnectionStringByDomainAsync(string domain)
        {
            var result = await context.Tenants.SingleAsync(t => t.Domain == domain.ToLowerInvariant());
            return new DatabaseCredentials(result.DatabaseUsername, result.DatabasePassword);
        }

        public async Task AddTenant(Tenant tenant)
        {
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }

        public async Task UpdateLastAccessed(string domain)
        {
            Tenant tenant = await FindTenantByDomainAsync(domain);
            tenant.LastAccessed = DateTime.UtcNow;
            context.Update(tenant);
            await context.SaveChangesAsync();
        }

        public async Task Delete(Tenant tenant)
        {
            context.Remove(tenant);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTenantAsync(Tenant tenant)
        {
            context.Update(tenant);
            await context.SaveChangesAsync();
        }
    }
}