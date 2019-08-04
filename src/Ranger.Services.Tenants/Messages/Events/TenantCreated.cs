using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants {
    [MessageNamespace ("tenants")]
    public class TenantCreated : IEvent {
        public TenantCreated (string domainName, string databaseUsername, string databasePassword, NewTenantOwner owner) {
            this.DomainName = domainName;
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
            this.Owner = owner;

        }
        public string DomainName { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
        public NewTenantOwner Owner { get; }
    }
}