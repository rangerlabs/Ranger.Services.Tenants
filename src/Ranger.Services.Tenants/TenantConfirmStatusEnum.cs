
namespace Ranger.Services.Tenants
{
    public enum TenantConfirmStatusEnum
    {
        PreviouslyConfirmed = 0,
        InvalidToken = 1,
        Confirmed = 2,
        TenantNotFound = 3
    }
}