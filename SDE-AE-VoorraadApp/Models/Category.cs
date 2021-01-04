using System.Collections.Generic;

namespace SDE_AE_VoorraadApp.Models
{
    public class Category
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}