using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{
    [MessageNamespace("tenants")]
    public class DeleteTenant : ICommand
    {
        public DeleteTenant(string commandingUserEmail, string domain)
        {
            this.CommandingUserEmail = commandingUserEmail;
            this.Domain = domain;
        }
        public string CommandingUserEmail { get; }
        public string Domain { get; }
    }
}