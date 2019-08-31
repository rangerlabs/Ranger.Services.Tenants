using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{
    [MessageNamespace("tenants")]
    public class DeleteTenant : ICommand
    {
        public DeleteTenant(string domain)
        {
            this.Domain = domain;
        }
        public string Domain { get; }
    }
}