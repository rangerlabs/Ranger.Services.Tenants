using System;
using Ranger.Services.Tenants.Data;

namespace Ranger.Services.Tenants
{
    public class TenantResponseModel
    {
        public string TenantId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string OrganizationName { get; set; }
        public string Domain { get; set; }
        public string DatabasePassword { get; set; }
        public bool Confirmed { get; set; }
        public PrimaryOwnerTransfer PrimaryOwnerTransfer { get; set; }
        public int Version { get; set; }
    }
}