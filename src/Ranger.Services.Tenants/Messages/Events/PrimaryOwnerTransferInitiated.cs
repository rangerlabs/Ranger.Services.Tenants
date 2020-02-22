using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{
    [MessageNamespace("tenants")]
    public class PrimaryOwnerTransferInitiated : IEvent
    {

    }
}