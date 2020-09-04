using System.Threading.Tasks;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{
    public interface ITenantService
    {
        Task<TenantResponseModel> GetTenantResponseModelOrDefaultFromRedisByIdAsync(string tenantId);
        Task<TenantResponseModel> GetTenantResponseModelOrDefaultFromRedisByDomainAsync(string domain);
        void SetTenantResponseModelInRedisById(TenantResponseModel model);
        void SetTenantResponseModelInRedisByDomain(TenantResponseModel model);
        Task RemoveTenantResponseModelsFromRedis(string tenantId, string domain);
        Task<TenantConfirmStatusEnum> ConfirmTenantAsync(string domain, string token);
        Task<(Tenant tenant, bool domainWasUpdated, string oldDomain)> UpdateTenantOrganizationDetailsAsync(string tenantId, string commandingUserEmail, int version, string organizationName = "", string domain = "");
    }
}
