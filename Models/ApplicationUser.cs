using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NaturalShop.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = null!;
        public string? Address { get; set; }
    }
}
