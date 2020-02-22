using System;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public class PrimaryOwnerTransfer
    {
        public PrimaryOwnerTransfer(DateTime initiatedAt, string initiatedByEmail, string transferingToEmail, PrimaryOwnerTransferStateEnum state, string correlationId)
        {
            this.InitiatedAt = initiatedAt;
            this.InitiatedByEmail = initiatedByEmail;
            this.TransferingToEmail = transferingToEmail;
            this.State = state;
            this.CorrelationId = correlationId;
        }

        public DateTime InitiatedAt { get; set; }
        public string InitiatedByEmail { get; set; }
        public string TransferingToEmail { get; set; }
        public PrimaryOwnerTransferStateEnum State { get; set; }
        public string CorrelationId { get; set; }

        public static PrimaryOwnerTransfer Create(string initiatedByEmail, string transferingToEmail, string correlationId)
        {
            return new PrimaryOwnerTransfer(DateTime.UtcNow, initiatedByEmail, transferingToEmail, PrimaryOwnerTransferStateEnum.Pending, correlationId);
        }
    }
}