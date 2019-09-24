using System.Threading.Tasks;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{

    public class TenantService : ITenantService
    {
        private readonly ITenantRepository tenantRepository;

        public TenantService(ITenantRepository tenantRepository)
        {
            this.tenantRepository = tenantRepository;
        }

        public async Task<TenantConfirmStatusEnum> ConfirmTenantAsync(string domain, string registrationKey)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"Argument '{nameof(domain)}' was null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(registrationKey))
            {
                throw new System.ArgumentException($"Argument '{nameof(registrationKey)}' was null or whitespace.");
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

            if (tenant.RegistrationKey == registrationKey)
            {
                tenant.RegistrationKey = "";
                tenant.Enabled = true;
                await tenantRepository.UpdateTenantAsync(tenant);
                return TenantConfirmStatusEnum.Confirmed;
            }
            return TenantConfirmStatusEnum.InvalidRegistrationKey;
        }
    }
}
