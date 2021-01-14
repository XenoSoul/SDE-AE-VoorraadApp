using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using SDE_AE_VoorraadApp.Models;

// TODO: Documentation
namespace SDE_AE_VoorraadApp.Data
{
    /// <summary>
    /// 
    /// </summary>
    public static class DbUpdater
    {
        /// <summary>
        /// Updates the AvailableCount and RefillAdviceCount fields within the ProductStock table of the LocationContext Database.
        /// </summary>
        /// <param name="context">
        /// Context of the LocationContext Database.
        /// </param>
        /// <returns>
        /// The number of affected lines.
        /// </returns>
        public static async Task<int> TwinkUpdate(LocationContext context)
        {
            // Check if the database exists.
            // If not initialize the database.
            if (!context.Locations.Any())
            {
                DbInitializer.Initialize(context);
            }

            // Copy context as otherwise the code would throw a fit.
            var _context = context;

            // Get all IDs of Machines in the LocationContext Database.
            var allMachineIds = context.Machines.Select(m => m.MachineId).ToArray();

            // Get all the ProductStock currently in the LocationContext Database that falls under the Machines provided.
            // It does this by sending GET requests to the VendingWebAPI with the MachineID previously acquired from the Machines table and loops over them.
            // And then finally this list of ProductStock gets send to the AsyncDbProductStockMachineProductLinkerWrapper in order to update the AvailableCount and RefillAdviceCount.
            var _productStocks = allMachineIds.Select(async machineId =>
                JsonSerializer.Deserialize<DbInitializer._ProductStock>(
                    (await AsyncApiRequester("machines/stock", $"{machineId}")).Content) ??
                throw new InvalidOperationException()).ToList();
            var currentProductStock = await
                AsyncDbProductStockMachineProductLinkerWrapper(_productStocks,
                     _context);

            context.ProductStocks.UpdateRange(currentProductStock);
            
            // Save the made changes and return the number of rows affected by the TwinkUpdate.
            return await context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes all the elements in the LocationContext Database and fires the DbInitializer.Initialize function in order to repopulate them.
        /// </summary>
        /// <param name="context">
        /// Context of the LocationContext Database.
        /// </param>
        public static void TwonkUpdate(LocationContext context)
        {
            // Remove all elements from tables as context.TABLENAME.ToArray() are always all the elements in that table.
            context.ProductStocks.RemoveRange(context.ProductStocks.ToArray());
            context.SaveChanges();
            context.Products.RemoveRange(context.Products.ToArray());
            context.SaveChanges();
            context.Categories.RemoveRange(context.Categories.ToArray());
            context.SaveChanges();
            context.Machines.RemoveRange(context.Machines.ToArray());
            context.SaveChanges();
            context.Locations.RemoveRange(context.Locations.ToArray());
            context.SaveChanges();

            // Reinitialize the Database
            DbInitializer.Initialize(context);
        }

        /// <summary>
        /// AsyncDbProductStockMachineProductLinkerWrapper exists to asynchronously transform a list of Task DbInitializer._ProductStocks into a Task&lt;List&lt;ProductStock&gt;&gt;.
        /// <para>The primary purpose of this function is to be used in the TwinkUpdate function so that it can asynchronously update the required fields in ProductStock.</para>
        /// </summary>
        /// <param name="newList_ProductStock">
        /// A list of asynchronously created _ProductStock to be used to update fields in the ProductStock table in the LocationContext Database.
        /// </param>
        /// <param name="context">
        /// Context of the Location context.
        /// </param>
        /// <returns>
        /// A complete list of ProductStock recommended to be used to update the LocationContext Database.
        /// </returns>
        private static async Task<List<ProductStock>> AsyncDbProductStockMachineProductLinkerWrapper(List<Task<DbInitializer._ProductStock>> newList_ProductStock, LocationContext context)
        {
            var _context = context;

            // Get all the new ProductStock from the API in the correct formatting.
            var newProductStocks = await AsyncDbProductStockMachineProductLinker(newList_ProductStock, _context);

            // Here the Machines get linked to the oldProductStocks to be used later in the function.
            var oldProductStocks = context.ProductStocks.ToList();
            foreach (var oldProductStock in oldProductStocks)
            {
                oldProductStock.Machine = context.Machines.ToList().Find(cm => cm.ID.Equals(oldProductStock.MachineId));
            }

            // Update the complete list of ProductStock with the ones from the updated in newProductStock.
            // Doing so requires finding the index of the oldProductStocks that needs to be update and using that index to update the amount.
            foreach (var newProductStock in newProductStocks)
            {
                var currentOldProductStockIndex = oldProductStocks.FindIndex(os =>
                    os.ProductId.Equals(newProductStock.ProductId) && os.MachineId.Equals(newProductStock.MachineId));

                oldProductStocks[currentOldProductStockIndex].AvailableCount = newProductStock.AvailableCount;
                oldProductStocks[currentOldProductStockIndex].RefillAdviceCount = newProductStock.RefillAdviceCount;
            }

            // Return the updated list.
            return oldProductStocks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header">
        ///
        /// </param>
        /// <param name="subheader">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        private static async Task<IRestResponse> AsyncApiRequester(string header, string subheader)
        {
            var client = new RestClient($"https://api-staging-vendingweb.azurewebsites.net/api/external/{header}/")
            {
                Authenticator = new HttpBasicAuthenticator("AppelEitjeTest", "appeleitje@!78")
            };

            var request = new RestRequest($"{subheader}", DataFormat.Json);

            return await client.ExecuteGetAsync(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productStocks"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task<List<ProductStock>> AsyncDbProductStockMachineProductLinker(List<Task<DbInitializer._ProductStock>> productStocks, LocationContext context)
        {
            var products = context.Products.ToList();
            var machines = context.Machines.ToList();

            var list = new List<ProductStock>();
            foreach (var result in productStocks.ToList())
            {
                var tResult = await result;
                list.AddRange(tResult.ProductStock.Select(cProductStock => new ProductStock
                {
                    ID = 0,
                    ProductId = products.Find(p => p.ProductId.Equals(cProductStock.ProductId)).ID,
                    MachineId = machines.Find(x => x.MachineId.Equals(tResult.MachineId)).ID,
                    AvailableCount = cProductStock.AvailableCount,
                    MinCount = cProductStock.MinCount,
                    MaxCount = cProductStock.MaxCount,
                    RefillAdviceCount = cProductStock.RefillAdviceCount
                }));
            }
            
            return list;
        }
    }
}