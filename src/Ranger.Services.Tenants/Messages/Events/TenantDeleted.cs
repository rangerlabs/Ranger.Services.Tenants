using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{
    [MessageNamespace("tenants")]
    public class TenantDeleted : IEvent
    {
        public TenantDeleted(string domainName)
        {
            this.DomainName = domainName;

        }
        public string DomainName { get; }
    }
}