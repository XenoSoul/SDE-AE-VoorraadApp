using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SDE_AE_VoorraadApp.Models
{
    public class Location
    {
        public int ID { get; set; }
        [JsonPropertyName("Location")]
        public string Place { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public ICollection<Machine> Machines { get; set; }
    }
}
