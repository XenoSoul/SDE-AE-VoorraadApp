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
    /// <summary>
    /// The <c>DbInitializer</c>
    /// </summary>
    /// <para></para>
    public static class DbInitializer
    {
        /// <summary>
        /// Initialize is the main function of DbInitializer.
        /// The brunt of this function is fired if the database has not yet been seeded. Otherwise it just return to its normal operations.
        /// </summary>
        /// <param name="context">
        /// The context of the LocationDatabase used to connect to the database and save appropriate changes.
        /// </param>
        public static void Initialize(LocationContext context)
        {
            // Makes sure the database is there.
            context.Database.EnsureCreated();

            // Checks if the database is already seeded.
            if (context.Locations.Any())
                return;

            // Get all the Locations from the VendingWeb API.
            var locations = JsonSerializer.Deserialize<List<Location>>(ApiRequester("machines", "").Content) ?? throw new InvalidOperationException();
            // Filters out all the unique locations as Locations needs to be extracted from Machines.
            locations = UniqueLocationsFilter(locations);
            // Adds the locations to the Locations part of the database alphabetically ordered the city name
            context.Locations.AddRange(locations.OrderBy(l => l.City));
            context.SaveChanges();

            // Get all the Categories from the VendingWeb API.
            var categories = JsonSerializer.Deserialize<List<Category>>(ApiRequester("products", "categories").Content) ?? throw new InvalidOperationException();
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // Get all the different Products from the VendingWeb API.
            var products = JsonSerializer.Deserialize<List<Product>>(ApiRequester("products", "").Content) ?? throw new InvalidOperationException();
            context.Products.AddRange(DbProductCatagoryLinker(products, context.Categories.ToList()));
            context.SaveChanges();

            // Get all the different Machines from the VendingWeb API.
            var machines = JsonSerializer.Deserialize<List<_Machine>>(ApiRequester("machines", "").Content) ?? throw new InvalidOperationException();
            context.Machines.AddRange(DbMachineLocationLinker(machines, context.Locations.ToList()));
            context.SaveChanges();

            // Get all the indexes from all the machines to be used to update the machineIds of ProductStocks to avoid Reference exception.
            var allMachineIds = context.Machines.Select(m => m.MachineId).ToArray();
            var _productStocks = allMachineIds.Select(machineId =>
                JsonSerializer.Deserialize<_ProductStock>(ApiRequester("machines/stock", $"{machineId}").Content) ??
                throw new InvalidOperationException()).ToList();
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

        /// <summary>
        /// UniqueLocationsFilter takes a list of Locations and checks if the list contains duplicates.
        /// </summary>
        /// <param name="locations">
        /// Used to return all the machines cast into Locations.
        /// </param>
        /// <returns>
        /// A list of unique Locations.
        /// </returns>
        private static List<Location> UniqueLocationsFilter(List<Location> locations)
        {
            // Make a copy of all the Machines so that that list can be edited safely.
            var _locations = locations;

            foreach (var _location in _locations.ToList())
            {
                // Figure out how many machines are of the same location.
                // This is done by looking at the Latitude & Longitude and if they're the same add them to a list, of which we then take the total count.
                // If this count is greater then 1 then the other duplicates get deleted (This is done in order to improve efficiency in runtime).
                // The deletion of the duplicates is done an amount of times equal to the total amount of duplicates - 1.
                var locationDupCount = locations.FindAll(x => Math.Abs(x.Latitude - _location.Latitude) < 1 && Math.Abs(x.Longitude - _location.Longitude) < 1).Count;
                if (locationDupCount <= 1) continue;
                for (var i = 0; i < locationDupCount - 1; i++)
                {
                    locations.Remove(_location);
                }
            }

            // Return the remaining unique Locations
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
                product.CategoryId = categories.Find(x => x.CategoryID.Equals(product.CategoryId)).ID;
            }

            return products;
        }

        private static List<ProductStock> DbProductStockMachineProductLinker(List<_ProductStock> productStocks, LocationContext context)
        {
            return (productStocks.ToList()
                .SelectMany(productStock => productStock.ProductStock,
                    (productStock, _productStock) => new ProductStock
                    {
                        ID = 0,
                        ProductId = context.Products.ToList().Find(p => p.ProductId.Equals(_productStock.ProductId)).ID,
                        MachineId = context.Machines.ToList().Find(x => x.MachineId.Equals(productStock.MachineId)).ID,
                        AvailableCount = _productStock.AvailableCount,
                        MinCount = _productStock.MinCount,
                        MaxCount = _productStock.MaxCount,
                        RefillAdviceCount = _productStock.RefillAdviceCount
                    })).ToList();
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