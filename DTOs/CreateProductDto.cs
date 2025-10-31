using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NaturalShop.API.DTOs
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [DefaultValue(0)]
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be 0 or greater")]
        [DefaultValue(0)]
        public int? Stock { get; set; }
    }
}


