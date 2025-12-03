using System.ComponentModel.DataAnnotations;

namespace NaturalShop.API.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; } = null!;
        
        [Required]
        [MinLength(8)]
        [MaxLength(50)]
        public string Password { get; set; } = null!;
    }
}
