using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SDE_AE_VoorraadApp.Models
{
    public class Machine
    {
        [JsonIgnore]
        public int ID { get; set; }
        public int MachineId { get; set; }
        public string Name { get; set; }
        public int LocationID { get; set; }

        public Location Location { get; set; }
        public ICollection<ProductStock> ProductStocks { get; set; }

    }
}