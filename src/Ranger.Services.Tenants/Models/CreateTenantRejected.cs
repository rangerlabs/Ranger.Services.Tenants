using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants {
    internal class CreateTenantRejected : IRejectedEvent {
        public string Reason { get; set; }
        public string Code { get; set; }
        public CorrelationContext CorrelationContext { get; set; }

        public CreateTenantRejected (CorrelationContext correlationContext, string reason, string code) {
            CorrelationContext = correlationContext;
            this.Reason = reason;
        }
    }
}