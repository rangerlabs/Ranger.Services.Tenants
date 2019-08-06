using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants {
    public class CreateTenantRejected : IRejectedEvent {
        public string Reason { get; }
        public string Code { get; }

        public CreateTenantRejected (string message, string code) {
            this.Reason = message;
            this.Code = code;
        }
    }
}