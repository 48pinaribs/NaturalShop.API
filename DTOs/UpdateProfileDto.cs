using System.ComponentModel.DataAnnotations;

namespace NaturalShop.API.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [MinLength(2)]
        public string FullName { get; set; } = null!;

        public string? Address { get; set; }
    }
}
