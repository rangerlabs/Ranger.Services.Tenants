using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants {

    [MessageNamespace ("tenants")]
    public class CreateTenant : ICommand {
        public CreateTenant (Domain domain, User user) {
            this.Domain = domain;
            this.User = user;

        }
        public Domain Domain { get; }
        public User User { get; }
    }
}