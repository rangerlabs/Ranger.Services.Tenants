using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ranger.Services.Tenants.Data
{
    public class TenantStream
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public Guid StreamId { get; set; }
        [Required]
        public int Version { get; set; }
        [Required]
        [Column(TypeName = "jsonb")]
        public string Data { get; set; }
        [Required]
        public string Event { get; set; }
        [Required]
        public DateTime InsertedAt { get; set; }
        [Required]
        public string InsertedBy { get; set; }
    }
}