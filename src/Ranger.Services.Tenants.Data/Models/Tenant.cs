using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data {
    public class Tenant {
        [Key]
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccessed { get; set; }

        [StringLength (28, MinimumLength = 3)]
        [RegularExpression (@"^[a-zA-Z0-9]{1}[a-zA-Z0-9_ ]{1,26}[a-zA-Z0-9]{1}$")]
        public string OrganizationName { get; set; }

        [StringLength (28, MinimumLength = 4)]
        [RegularExpression (@"^[a-zA-Z0-9]{1}[a-zA-Z0-9-]{1,26}[a-zA-Z0-9]{1}$")]
        public string Domain { get; set; }

        [Encrypted]
        public string DatabaseUsername { get; set; }

        [Encrypted]
        public string DatabasePassword { get; set; }

        [MaxLength (64)]
        public string RegistrationKey { get; set; }
        public bool DomainConfirmed { get; set; }
    }
}