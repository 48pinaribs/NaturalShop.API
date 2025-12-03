using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NaturalShop.API.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order? Order { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        [JsonIgnore] // Döngüsel referansı önlemek için JSON serialization'dan hariç tutulur
        public Product? Product { get; set; }

    }
}
