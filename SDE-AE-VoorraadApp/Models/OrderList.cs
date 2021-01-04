using System;
using System.Collections.Generic;

namespace SDE_AE_VoorraadApp.Models
{
    public class OrderList
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime DateTimeCreated { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}