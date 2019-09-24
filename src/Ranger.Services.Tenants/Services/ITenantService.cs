using System.Threading.Tasks;

namespace Ranger.Services.Tenants
{
    public interface ITenantService
    {
        Task<TenantConfirmStatusEnum> ConfirmTenantAsync(string domain, string registrationKey);
    }
}
