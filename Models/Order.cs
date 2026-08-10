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

        // Teslimat bilgileri - checkout formundan gelir
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? ShippingAddress { get; set; }

        // Kargo durumu - Status (ödeme durumu) alanından bağımsız, admin panelinden güncellenir
        // Değerler: "Hazırlanıyor" (varsayılan) -> "Kargoya Verildi" -> "Teslim Edildi"
        public string ShippingStatus { get; set; } = "Hazırlanıyor";
        public string? TrackingNumber { get; set; }
    }
}
