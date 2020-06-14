using System.Threading.Tasks;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{
    public interface ITenantService
    {
        Task<TenantConfirmStatusEnum> ConfirmTenantAsync(string domain, string token);
        Task<(Tenant tenant, bool domainWasUpdated, string oldDomain)> UpdateTenantOrganizationDetailsAsync(string tenantId, string commandingUserEmail, int version, string organizationName = "", string domain = "");
    }
}
