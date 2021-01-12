using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDE_AE_VoorraadApp.Models
{
    public class ProductStock
    {
        public int ID { get; set; }
        public int ProductId { get; set; }
        public int MachineId { get; set; }
        public int AvailableCount { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public int RefillAdviceCount { get; set; }

        public Machine Machine { get; set; }
        public Product Product { get; set; }
        public ICollection<Order> Orders { get; set; }

    }
}