using System.ComponentModel.DataAnnotations;

namespace Ranger.Services.Tenants
{
    public class ConfirmModel
    {
        [Required]
        public string Token { get; set; }
    }
}