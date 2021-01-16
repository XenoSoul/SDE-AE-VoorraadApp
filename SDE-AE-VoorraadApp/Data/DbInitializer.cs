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
    /// The DbInitializer class is the main class used to seed the the LocationContext Database.
    /// <para>It does this by way of requesting the VendingWebAPI and its many components.</para>
    /// </summary>
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
            context.Products.AddRange(DbProductCategoryLinker(products, context.Categories.ToList()));
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
                var locationDupCount = locations.FindAll(x => Math.Abs(x.Latitude - _location.Latitude) < 0.0001 && Math.Abs(x.Longitude - _location.Longitude) < 0.0001).Count;
                if (locationDupCount <= 1) continue;
                for (var i = 0; i < locationDupCount - 1; i++)
                {
                    locations.Remove(_location);
                }
            }

            // Return the remaining unique Locations
            return locations;
        }

        /// <summary>
        /// DbMachineLocationLinker changes the list of _Machines into a list of Machines by setting their LocationID.
        /// It does this by looking finding a Location from which the Latitude and Longitude are the same as itself.
        /// </summary>
        /// <param name="machines">
        /// A list of Machines of which the LocationID has yet to be set.
        /// </param>
        /// <param name="locations">
        /// A list of all location currently in the LocationContext Database.
        /// </param>
        /// <returns>
        /// A list of Machines ready to be seeded into the LocationContext Database.
        /// </returns>
        private static List<Machine> DbMachineLocationLinker(List<_Machine> machines, List<Location> locations)
        {
            // Create a list of Machines for machines to be cast into.
            // After this is loops over all the _Machine in machines and finds a location that is the same as the Machine Latitude and Longitude.
            // Finally it adds this new Machine into the legitMachines list.
            // It does this till all the machines from machines have been looped over.
            var legitMachines = new List<Machine>();
            foreach (var machine in machines.ToList())
            {
                machine.LocationID = locations.Find(x => Math.Abs(x.Latitude - machine.Latitude) < 1 && Math.Abs(x.Longitude - machine.Longitude) < 1).ID;
                legitMachines.Add(new Machine { ID = 0, LocationID = machine.LocationID, MachineId = machine.MachineID, Name = machine.Name });
            }

            // returns the list of Machines.
            return legitMachines;
        }

        /// <summary>
        /// DbProductCategoryLinker Updates all the CategoryIDs from products.
        /// This is done in order to avoid concurrency issues and the Product.CategoryID refer to an Category.ID that does not exist.
        /// </summary>
        /// <param name="products">
        /// List of Products that are to be looped over and updated.
        /// </param>
        /// <param name="categories">
        /// List of all the categories in the LocationContext Database.
        /// </param>
        /// <returns>
        /// The updated list of Products with correct Product.CategoryIDs.
        /// </returns>
        private static List<Product> DbProductCategoryLinker(List<Product> products, List<Category> categories)
        {
            // Loop over products in order to update the CategoryID.
            foreach (var product in products.ToList())
            {
                product.CategoryId = categories.Find(x => x.CategoryID.Equals(product.CategoryId)).ID;
            }

            // Returns the updated Products
            return products;
        }

        /// <summary>
        /// DbProductStockMachineProductLinker changes the ProductId and MachineId from the List of _ProductStock in order to update them to their correct values.
        /// After having done so DbProductStockMachineProductLinker returns a list of ProductStock with the correct values.
        /// </summary>
        /// <param name="productStocks">
        /// A list of _ProductStock with to be updated ProductId and MachineId.
        /// </param>
        /// <param name="context">
        /// Context of the LocationContext Database.
        /// </param>
        /// <returns>
        /// A list of ProductStock with correct Ids.
        /// </returns>
        private static List<ProductStock> DbProductStockMachineProductLinker(List<_ProductStock> productStocks, LocationContext context)
        {
            // Using LINQ we find the ProductStock which corresponds with the machine and product with the same ProductId and MachineId.
            // After this we create a new ProductStock with tne correct values and that to the total list.
            // Finally we return this list.
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

        /// <summary>
        /// ApiRequester is the lifeblood of this object and the functions within.
        /// This function uses functions from the RestSharp library in order to send GET requests to the VendingWebAPI.
        /// </summary>
        /// <param name="header">
        /// Divines what part of the API it will request data from.
        /// </param>
        /// <param name="subheader">
        /// Details which part of the API within the header is queried. 
        /// </param>
        /// <returns>
        /// A GET request IRestResponse.
        /// </returns>
        private static IRestResponse ApiRequester(string header, string subheader)
        {
            // Setting up the client that the function will send a request to.
            // It does this by taking the header input and setting that as the RestClient.
            var client = new RestClient($"https://api-staging-vendingweb.azurewebsites.net/api/external/{header}/")
            {
                // Although not particularly safe the Authenticator of the Client gets gets divined here in order to be able to access the API.
                Authenticator = new HttpBasicAuthenticator("AppelEitjeTest", "appeleitje@!78")
            };

            // Set up the request that will be send.
            // Here the return DataFormat also gets defined, being JSON in this case.
            var request = new RestRequest($"{subheader}", DataFormat.Json);

            // Send the actual GET request and return the outcome.
            return client.Get(request);
        }

        /// <summary>
        /// _Machine is used in order to deserialize the different machines from the VendingWeb API.
        /// This is so that later on the LocationID can be changed in accordance with the LocationContext Database.
        /// </summary>
        private class _Machine
        {
            // Define that this value should be seen as the ID from the received JSON.
            [JsonPropertyName("Id")]
            public int MachineID { get; set; }
            public string Name { get; set; }
            public int LocationID { get; set; }
            // Define that this value should be seen as the Location from the received JSON.
            // Being that they have different names this causes conflicts while deserializing the content from the VendingWeb API.
            [JsonPropertyName("Location")]
            public string Place { get; set; }
            public float Latitude { get; set; }
            public float Longitude { get; set; }
        }

        /// <summary>
        /// Exists in order to simulate the way the ProductStock gets received from the VendingWeb API
        /// </summary>
        public class _ProductStock
        {
            public int MachineId { get; set; }
            public List<ProductStock> ProductStock { get; set; }
           
        }
    }
}