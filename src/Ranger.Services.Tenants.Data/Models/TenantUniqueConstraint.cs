using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ranger.Services.Tenants.Data
{
    public class TenantUniqueConstraint
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TenantId { get; set; }

        [Required]
        [StringLength(28, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]{1}[a-zA-Z0-9-]{1,26}[a-zA-Z0-9]{1}$")]
        public string Domain { get; set; }
    }
}