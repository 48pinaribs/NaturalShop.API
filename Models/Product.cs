using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NaturalShop.API.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public int Stock { get; set; }
        public string? StoryText { get; set; }
        // StoryImages is an array of strings
        public string[]? StoryImages { get; set; }
        public string Code { get; set; } = string.Empty;
        // Images array for gallery
        public string[]? Images { get; set; }

        // Navigation property (bir ürün birçok OrderItem'a gider)
        // JsonIgnore: Döngüsel referansı önlemek için JSON serialization'dan hariç tutulur
        [JsonIgnore]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}


