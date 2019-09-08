using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ranger.Common;

namespace Ranger.Services.Tenants.Data
{
    public class Tenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }
        public DateTime? LastAccessed { get; set; }

        [Required]
        [StringLength(28, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]{1}[a-zA-Z0-9- ]{1,26}[a-zA-Z0-9]{1}$")]
        public string OrganizationName { get; set; }

        [Required]
        [StringLength(28, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]{1}[a-zA-Z0-9-]{1,26}[a-zA-Z0-9]{1}$")]
        public string Domain { get; set; }

        [Required]
        [Encrypted]
        public string DatabaseUsername { get; set; }

        [Required]
        [Encrypted]
        public string DatabasePassword { get; set; }

        [Required]
        [MaxLength(64)]
        public string RegistrationKey { get; set; }

        [Required]
        public bool DomainConfirmed { get; set; }
    }
}