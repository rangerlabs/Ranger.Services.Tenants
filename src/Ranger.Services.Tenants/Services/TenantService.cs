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
                throw new System.ArgumentException($"'{nameof(domain)}' was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new System.ArgumentException($"'{nameof(token)}' was null or whitespace.");
            }

            Tenant tenant = await tenantRepository.FindTenantByDomainAsync(domain);
            if (tenant is null)
            {
                return TenantConfirmStatusEnum.TenantNotFound;
            }
            if (tenant.Enabled)
            {
                return TenantConfirmStatusEnum.PreviouslyConfirmed;
            }

            if (tenant.Token == token)
            {
                tenant.Token = "";
                tenant.Enabled = true;
                await tenantRepository.UpdateTenantAsync("Anonymous", "TenantConfirmed", 1, tenant);
                return TenantConfirmStatusEnum.Confirmed;
            }
            return TenantConfirmStatusEnum.InvalidToken;
        }
    }
}
