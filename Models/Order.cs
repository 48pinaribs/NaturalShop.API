using System;
using System.Collections.Generic;

namespace NaturalShop.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string CustomerAddress { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Hazırlanıyor";

        public List<OrderItem> Items { get; set; } = [];
    }
}
