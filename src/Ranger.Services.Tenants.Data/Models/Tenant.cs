using System;

namespace Ranger.Services.Tenants.Data
{
    public class Tenant
    {
        public string TenantId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccessed { get; set; }
        public string OrganizationName { get; set; }
        public string Domain { get; set; }
        public string DatabasePassword { get; set; }
        public string Token { get; set; }
        public bool Confirmed { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public PrimaryOwnerTransfer PrimaryOwnerTransfer { get; set; }
    }
}