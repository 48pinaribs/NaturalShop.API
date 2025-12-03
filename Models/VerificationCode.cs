using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NaturalShop.API.Models
{
    public class VerificationCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }
    }
}

