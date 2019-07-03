using Newtonsoft.Json;
using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants {
    internal class CreateTenant : ICommand {
        public CorrelationContext CorrelationContext { get; set; }
        public string OrganizationName { get; set; }
        public string Domain { get; set; }
    }
}