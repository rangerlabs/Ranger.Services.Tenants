using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants {
    internal class CreateTenantRejected : IRejectedEvent {
        public CorrelationContext CorrelationContext { get; }
        public string Reason { get; }
        public string Code { get; }

        public CreateTenantRejected (CorrelationContext correlationContext, string message, string code) {
            this.CorrelationContext = correlationContext;
            this.Reason = message;
            this.Code = code;
        }
    }
}