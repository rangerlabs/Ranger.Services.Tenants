using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public class Tenant
    {
        public Guid TenantId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccessed { get; set; }
        public string OrganizationName { get; set; }
        public string Domain { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string Token { get; set; }
        public bool Enabled { get; set; } = false;
        public bool Deleted { get; set; } = false;
    }
}