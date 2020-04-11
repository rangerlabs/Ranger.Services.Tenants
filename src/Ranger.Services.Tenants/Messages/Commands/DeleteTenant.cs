using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{
    [MessageNamespace("tenants")]
    public class DeleteTenant : ICommand
    {
        public DeleteTenant(string commandingUserEmail, string tenantId)
        {
            this.CommandingUserEmail = commandingUserEmail;
            this.TenantId = tenantId;
        }
        public string CommandingUserEmail { get; }
        public string TenantId { get; }
    }
}