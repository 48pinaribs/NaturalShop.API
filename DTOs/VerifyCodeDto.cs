using System.ComponentModel.DataAnnotations;

namespace NaturalShop.API.DTOs
{
    public class VerifyCodeDto
    {
        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [RegularExpression(@"^90\d{10}$", ErrorMessage = "Geçerli bir telefon numarası giriniz (90XXXXXXXXXX formatında)")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Doğrulama kodu gereklidir")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "Doğrulama kodu 4-10 karakter arasında olmalıdır")]
        public string Code { get; set; } = null!;
    }
}

