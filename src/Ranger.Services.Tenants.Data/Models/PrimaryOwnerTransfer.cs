using System;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public class PrimaryOwnerTransfer
    {
        public PrimaryOwnerTransfer(DateTime initiatedAt, string initiatedByEmail, string transferingToEmail, PrimaryOwnerTransferStateEnum state)
        {
            this.InitiatedAt = initiatedAt;
            this.InitiatedByEmail = initiatedByEmail;
            this.TransferingToEmail = transferingToEmail;
            this.State = state;
        }

        public DateTime InitiatedAt { get; set; }
        public string InitiatedByEmail { get; set; }
        public string TransferingToEmail { get; set; }
        public PrimaryOwnerTransferStateEnum State { get; set; }

        public static PrimaryOwnerTransfer Create(string initiatedByEmail, string transferingToEmail)
        {
            return new PrimaryOwnerTransfer(DateTime.UtcNow, initiatedByEmail, transferingToEmail, PrimaryOwnerTransferStateEnum.Pending);
        }
    }
}