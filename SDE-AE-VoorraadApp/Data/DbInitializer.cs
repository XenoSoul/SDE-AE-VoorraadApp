using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;
using SDE_AE_VoorraadApp.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SDE_AE_VoorraadApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(LocationContext context)
        {
            context.Database.EnsureCreated();

            if (context.Locations.Any())
                return;

            var locations = JsonSerializer.Deserialize<List<Location>>(ApiRequester("Machines", "").Content) ?? throw new InvalidOperationException();
            locations = UniqueLocationsFilter(locations);
            context.Locations.AddRange(locations);
            context.SaveChanges();


            var machines = JsonSerializer.Deserialize<List<_Machine>>(ApiRequester("Machines", "").Content) ?? throw new InvalidOperationException();
            context.Machines.AddRange(DbMachineLocationLinker(machines, context.Locations.ToList()));
            context.SaveChanges();


            // TODO: Create API in order to fill the ProductStock table with data
            var productStocks = new ProductStock[]
            {

            };

            // TODO: Create API in order to fill the OrderList table with data
            var orderLists = new OrderList[]
            {

            };

            // TODO: Create API in order to fill the Order table with data
            var orders = new Order[]
            {

            };

        }

        private static List<Location> UniqueLocationsFilter(List<Location> locations)
        {
            var _locations = locations;

            foreach (var _location in _locations.ToList())
            {
                var locationDupCount = locations.FindAll(x => x.Place == _location.Place).Count;
                if (locationDupCount > 1)
                {
                    for (var i = 0; i < locationDupCount - 1; i++)
                    {
                        locations.Remove(_location);
                    }
                }
            }

            return locations;
        }

        private static List<Machine> DbMachineLocationLinker(List<_Machine> machines, List<Location> locations)
        {
            var legitMachines = new List<Machine>();

            foreach (var machine in machines.ToList())
            {
                machine.LocationID = locations.Find(x => x.Place == machine.Place).ID;
                var realMachine = new Machine{ID = 0, LocationID = machine.LocationID, MachineID = machine.MachineID, Name = machine.Name};
                legitMachines.Add(realMachine);
            }

            return legitMachines;
        }

        private static IRestResponse ApiRequester(string header, string subheader)
        {
            var client = new RestClient($"https://api-staging-vendingweb.azurewebsites.net/api/external/{header}/")
            {
                Authenticator = new HttpBasicAuthenticator("AppelEitjeTest", "appeleitje@!78")
            };

            var request = new RestRequest($"{subheader}", DataFormat.Json);

            return client.Get(request);
        }

        private class _Machine
        {
            [JsonPropertyName("Id")]
            public int MachineID { get; set; }
            public string Name { get; set; }
            public int LocationID { get; set; }
            [JsonPropertyName("Location")]
            public string Place { get; set; }
            public float Latitude { get; set; }
            public float Longitude { get; set; }
        }
    }
}