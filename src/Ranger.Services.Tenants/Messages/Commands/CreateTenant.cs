using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants {

    [MessageNamespace ("tenants")]
    public class CreateTenant : ICommand {
        public CreateTenant (CorrelationContext correlationContext, Domain domain, User user) {
            this.CorrelationContext = correlationContext;
            this.Domain = domain;
            this.User = user;

        }
        public CorrelationContext CorrelationContext { get; }
        public Domain Domain { get; }
        public User User { get; }
    }
}