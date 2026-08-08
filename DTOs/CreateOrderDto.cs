using System.Text.Json.Serialization;

namespace NaturalShop.API.DTOs
{
    public class CreateOrderDto
    {
        [JsonPropertyName("items")]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

        // Checkout formundan gelen teslimat bilgileri (opsiyonel - yoksa kullanıcı profilinden alınır)
        [JsonPropertyName("recipientName")]
        public string? RecipientName { get; set; }

        [JsonPropertyName("recipientPhone")]
        public string? RecipientPhone { get; set; }

        [JsonPropertyName("shippingAddress")]
        public string? ShippingAddress { get; set; }
    }

    public class OrderItemDto
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
