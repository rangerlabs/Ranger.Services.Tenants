using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{
    public class DeleteTenantRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public DeleteTenantRejected(string message, string code)
        {
            this.Reason = message;
            this.Code = code;
        }
    }
}