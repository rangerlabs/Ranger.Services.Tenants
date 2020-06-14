using System;
using System.Threading.Tasks;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{

    public class TenantsService : ITenantService
    {
        private readonly ITenantsRepository tenantRepository;

        public TenantsService(ITenantsRepository tenantRepository)
        {
            this.tenantRepository = tenantRepository;
        }

        public async Task<TenantConfirmStatusEnum> ConfirmTenantAsync(string domain, string token)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"'{nameof(domain)}' was null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentException($"'{nameof(token)}' was null or whitespace");
            }

            var (tenant, _) = await tenantRepository.FindNotDeletedTenantByDomainAsync(domain);
            if (tenant is null)
            {
                return TenantConfirmStatusEnum.TenantNotFound;
            }
            if (tenant.Confirmed)
            {
                return TenantConfirmStatusEnum.PreviouslyConfirmed;
            }

            if (tenant.Token == token)
            {
                tenant.Token = "";
                tenant.Confirmed = true;
                await tenantRepository.UpdateTenantAsync("Anonymous", "TenantConfirmed", 1, tenant);
                return TenantConfirmStatusEnum.Confirmed;
            }
            return TenantConfirmStatusEnum.InvalidToken;
        }

        public async Task<(Tenant tenant, bool domainWasUpdated, string oldDomain)> UpdateTenantOrganizationDetailsAsync(string tenantId, string commandingUserEmail, int version, string organizationName = "", string domain = "")
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"'{nameof(commandingUserEmail)}' cannot be null or whitespace", nameof(commandingUserEmail));
            }

            if (string.IsNullOrWhiteSpace(organizationName) && string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"'{nameof(organizationName)}' or '{nameof(domain)}' must not null or whitespace", nameof(organizationName));
            }

            if (version <= 1)
            {
                throw new ArgumentException($"'{nameof(version)}' must be greater than 1", nameof(version));
            }

            var tenantVersion = await tenantRepository.FindNotDeletedTenantByTenantIdAsync(tenantId);
            if (!String.IsNullOrWhiteSpace(organizationName))
            {
                tenantVersion.tenant.OrganizationName = organizationName;
            }
            var domainWasUpdated = false;
            var oldDomain = "";
            if (!String.IsNullOrWhiteSpace(domain))
            {
                tenantVersion.tenant.Domain = domain;
                oldDomain = tenantVersion.tenant.Domain;
                domainWasUpdated = true;
            }

            await tenantRepository.UpdateTenantAsync(commandingUserEmail, "TenantOrganizationUpdated", version, tenantVersion.tenant);
            return (tenantVersion.tenant, domainWasUpdated, oldDomain);
        }
    }
}
