using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{
    [MessageNamespace("tenants")]
    public class TenantCreated : IEvent
    {
        public TenantCreated(string domainName, string databaseUsername, string databasePassword, string token, NewTenantOwner owner)
        {
            this.DomainName = domainName;
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
            this.Token = token;
            this.Owner = owner;

        }
        public string DomainName { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
        public string Token { get; }
        public NewTenantOwner Owner { get; }
    }
}