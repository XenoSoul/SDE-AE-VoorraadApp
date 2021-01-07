using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SDE_AE_VoorraadApp.Models
{
    public class Category
    {
        [JsonIgnore]
        public int ID { get; set; }
        [JsonPropertyName("Id")]
        public int CategoryID { get; set; }
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}