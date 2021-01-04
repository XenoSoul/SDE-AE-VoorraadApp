﻿using System.Collections.Generic;

namespace SDE_AE_VoorraadApp.Models
{
    public class Machine
    {
        public int ID { get; set; }
        public int MachineID { get; set; }
        public string Name { get; set; }
        public int LocationID { get; set; }

        public Location Location { get; set; }
        public ICollection<ProductStock> ProductStocks { get; set; }

    }
}