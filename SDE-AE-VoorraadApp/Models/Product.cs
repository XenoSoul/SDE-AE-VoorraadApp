using System.Collections.Generic;

namespace SDE_AE_VoorraadApp.Models
{
    public class Product
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string ProductSKU { get; set; }

        public ICollection<ProductStock> ProductStocks { get; set; }
        public Category Category { get; set; }
    }
}