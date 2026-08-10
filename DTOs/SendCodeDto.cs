using System.ComponentModel.DataAnnotations;

namespace NaturalShop.API.DTOs
{
    public class SendCodeDto
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = null!;
    }
}
