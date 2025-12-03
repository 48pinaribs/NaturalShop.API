using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NaturalShop.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Hazırlanıyor";

        public List<OrderItem> Items { get; set; } = [];

        public string UserId { get; set; } = null!;
        [JsonIgnore] // Döngüsel referansı önlemek için JSON serialization'dan hariç tutulur
        public ApplicationUser User { get; set; } = null!;

        public decimal TotalAmount { get; set; }
        public string? IyzipayToken { get; set; }
    }
}
