using System.ComponentModel.DataAnnotations;

namespace NaturalShop.API.DTOs
{
    public class AdminLoginDto
    {
        [Required]
        public string Password { get; set; } = null!;
    }

    public class UpdateShippingStatusDto
    {
        [Required]
        public string ShippingStatus { get; set; } = null!;
        public string? TrackingNumber { get; set; }
    }
}
