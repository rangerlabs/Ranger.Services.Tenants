using Ranger.RabbitMQ;

namespace Ranger.Services.Tenants
{
    [MessageNamespace("tenants")]
    public class CompletePrimaryOwnerTransferRejected : IRejectedEvent
    {
        public CompletePrimaryOwnerTransferRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;

        }
        public string Reason { get; }
        public string Code { get; }
    }
}