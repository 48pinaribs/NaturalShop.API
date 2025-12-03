using System.ComponentModel.DataAnnotations;

namespace NaturalShop.API.DTOs
{
    public class SendCodeDto
    {
        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [RegularExpression(@"^90\d{10}$", ErrorMessage = "Geçerli bir telefon numarası giriniz (90XXXXXXXXXX formatında)")]
        public string PhoneNumber { get; set; } = null!;
    }
}

