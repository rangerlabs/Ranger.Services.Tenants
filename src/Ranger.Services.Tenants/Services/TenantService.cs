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
    }
}
