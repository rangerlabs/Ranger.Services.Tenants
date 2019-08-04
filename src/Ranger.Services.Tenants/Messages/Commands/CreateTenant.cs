using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants {

    [MessageNamespace ("tenants")]
    public class CreateTenant : ICommand {
        public CreateTenant (Domain domain, NewTenantOwner owner) {
            this.Domain = domain;
            this.Owner = owner;

        }
        public Domain Domain { get; }
        public NewTenantOwner Owner { get; }
    }
}