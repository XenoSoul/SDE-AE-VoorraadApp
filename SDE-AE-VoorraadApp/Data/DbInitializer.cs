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

            var locations = JsonSerializer.Deserialize<List<Location>>(ApiRequester("machines", "").Content) ?? throw new InvalidOperationException();
            locations = UniqueLocationsFilter(locations);
            context.Locations.AddRange(locations.OrderBy(l => l.City));
            context.SaveChanges();

            var categories = JsonSerializer.Deserialize<List<Category>>(ApiRequester("products", "categories").Content) ?? throw new InvalidOperationException();
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = JsonSerializer.Deserialize<List<Product>>(ApiRequester("products", "").Content) ?? throw new InvalidOperationException();
            context.Products.AddRange(DbProductCatagoryLinker(products, context.Categories.ToList()));
            context.SaveChanges();

            var machines = JsonSerializer.Deserialize<List<_Machine>>(ApiRequester("machines", "").Content) ?? throw new InvalidOperationException();
            context.Machines.AddRange(DbMachineLocationLinker(machines, context.Locations.ToList()));
            context.SaveChanges();

            var allMachineIds = context.Machines.Select(m => m.MachineId).ToArray();
            var _productStocks = allMachineIds.Select(machineId => JsonSerializer.Deserialize<_ProductStock>(ApiRequester("machines/stock", $"{machineId}").Content) ?? throw new InvalidOperationException()).ToList();
            var productStocks = DbProductStockMachineProductLinker(_productStocks, context);
            context.ProductStocks.AddRange(productStocks);
            context.SaveChanges();

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
                var realMachine = new Machine{ID = 0, LocationID = machine.LocationID, MachineId = machine.MachineID, Name = machine.Name};
                legitMachines.Add(realMachine);
            }

            return legitMachines;
        }

        private static List<Product> DbProductCatagoryLinker(List<Product> products, List<Category> categories)
        {
            foreach (var product in products.ToList())
            {
                product.CategoryId = categories.Find(x => x.CategoryID == product.CategoryId).ID;
            }

            return products;
        }

        private static List<ProductStock> DbProductStockMachineProductLinker(List<_ProductStock> productStocks, LocationContext context)
        {
            return (from productStock in productStocks.ToList()
                from _productStock in productStock.ProductStock
                select new ProductStock
                {
                    ID = 0,
                    ProductId = context.Products.ToList().Find(p => p.ProductId == _productStock.ProductId).ID,
                    MachineId = context.Machines.ToList().Find(x => x.MachineId == productStock.MachineId).ID,
                    AvailableCount = _productStock.AvailableCount,
                    MinCount = _productStock.MinCount,
                    MaxCount = _productStock.MaxCount,
                    RefillAdviceCount = _productStock.RefillAdviceCount
                }).ToList();
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

        public class _ProductStock
        {
            public int MachineId { get; set; }
            public List<ProductStock> ProductStock { get; set; }
           
        }
    }
}