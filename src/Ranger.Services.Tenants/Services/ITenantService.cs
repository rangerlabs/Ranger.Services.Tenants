using System.Threading.Tasks;

namespace Ranger.Services.Tenants
{
    public interface ITenantService
    {
        Task<TenantConfirmStatusEnum> ConfirmTenantAsync(string domain, string token);
        Task<(Tenant tenant, bool domainWasUpdated)> UpdateTenantOrganizationDetailsAsync(string tenantId, string commandingUserEmail, int version, string organizationName = "", string domain = "");
    }
}
