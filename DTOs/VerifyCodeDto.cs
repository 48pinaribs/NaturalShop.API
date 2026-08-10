using System.ComponentModel.DataAnnotations;

namespace NaturalShop.API.DTOs
{
    public class VerifyCodeDto
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Doğrulama kodu gereklidir")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "Doğrulama kodu 4-10 karakter arasında olmalıdır")]
        public string Code { get; set; } = null!;
    }
}
