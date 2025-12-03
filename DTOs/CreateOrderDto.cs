using System.Text.Json.Serialization;

namespace NaturalShop.API.DTOs
{
    public class CreateOrderDto
    {
        [JsonPropertyName("items")]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
