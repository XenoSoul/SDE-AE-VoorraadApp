using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SDE_AE_VoorraadApp.Models
{
    public class Product
    {
        [JsonIgnore]
        public int ID { get; set; }
        [JsonPropertyName("Id")]
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string SkuCode { get; set; }

        public ICollection<ProductStock> ProductStocks { get; set; }
        public Category Category { get; set; }
    }
}