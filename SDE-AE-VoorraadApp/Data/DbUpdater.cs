﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using RestSharp;
using RestSharp.Authenticators;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Data
{
    /// <summary>
    /// The DbUpdater class.
    /// Contains methods used for updating The LocationContext.
    /// </summary>
    /// <remarks>
    /// This Class can both update the amounts of products available as well as the the whole LocationContext Database (with help of DbInitializer).
    /// </remarks>
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
            // Check if it is the first time Twinkupdate is fired that day.
            // AKA, is it the first time a list has been created that day?
            // If so update the entire database through TwonkUpdate.
            /*if (context.OrderLists.ToList().Any())
            {
                DbInitializer.Initialize(context);
            }*/

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
        /// 
        /// </summary>
        /// <param name="context">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        public static async Task<int> TwonkUpdate(LocationContext context)
        {
            //
            var currentMachines = AsyncApiRequester("machines", "");

            //
            var oldLocations = context.Locations.ToList();
            var newLocations = DbInitializer
                .UniqueLocationsFilter(JsonSerializer.Deserialize<List<Location>>((await currentMachines).Content))
                .OrderBy(l => l.City).ToList();

            oldLocations = CheckListLengthDbObjects(oldLocations, newLocations);

            for (var i = 0; i < newLocations.Count; i++)
            {
                oldLocations[i].Place = newLocations[i].Place;
                oldLocations[i].Latitude = newLocations[i].Latitude;
                oldLocations[i].Longitude = newLocations[i].Longitude;
                oldLocations[i].City = newLocations[i].City;
                oldLocations[i].Country = newLocations[i].Country;
            }

            context.Locations.UpdateRange(oldLocations);
            await context.SaveChangesAsync();

            //
            var oldMachines = context.Machines.ToList();
            var new_Machines =
                JsonSerializer.Deserialize<List<DbInitializer._Machine>>((await currentMachines).Content) ??
                throw new InvalidOperationException();
            var newMachines = DbInitializer.DbMachineLocationLinker(new_Machines, context.Locations.ToList());

            oldMachines = CheckListLengthDbObjects(oldMachines, newMachines);

            for (var i = 0; i < newMachines.Count; i++)
            {
                oldMachines[i].MachineId = newMachines[i].MachineId;
                oldMachines[i].Name = newMachines[i].Name;
                oldMachines[i].LocationID = newMachines[i].LocationID;
            }

            context.Machines.UpdateRange(oldMachines);
            await context.SaveChangesAsync();

            //
            var oldCategories = context.Categories.ToList();
            var newCategories =
                JsonSerializer.Deserialize<List<Category>>((await AsyncApiRequester("products", "categories"))
                    .Content) ?? throw new InvalidOperationException();
            oldCategories = CheckListLengthDbObjects(oldCategories, newCategories);

            for (var i = 0; i < newCategories.Count; i++)
            {
                oldCategories[i].CategoryID = newCategories[i].CategoryID;
                oldCategories[i].Name = newCategories[i].Name;
            }

            context.Categories.UpdateRange(oldCategories);
            await context.SaveChangesAsync();

            //
            var oldProducts = context.Products.ToList();
            var newProducts =
                DbInitializer.DbProductCategoryLinker(JsonSerializer.Deserialize<List<Product>>(
                    (await AsyncApiRequester("products", ""))
                    .Content) ?? throw new InvalidOperationException(), oldCategories);
            oldProducts = CheckListLengthDbObjects(oldProducts, newProducts);
            for (var i = 0; i < newProducts.Count; i++)
            {
                oldProducts[i].ProductId = newProducts[i].ProductId;
                oldProducts[i].CategoryId = newProducts[i].CategoryId;
                oldProducts[i].Name = newProducts[i].Name;
                oldProducts[i].SkuCode = newProducts[i].SkuCode;
            }

            context.Products.UpdateRange(oldProducts);
            await context.SaveChangesAsync();

            // 
            var allMachineIds = oldMachines.Select(m => m.MachineId).ToArray();
            var _productStocks = allMachineIds.Select(async machineId =>
                JsonSerializer.Deserialize<DbInitializer._ProductStock>(
                    (await AsyncApiRequester("machines/stock", $"{machineId}")).Content) ??
                throw new InvalidOperationException()).ToList();
            var newProductStock = await AsyncDbFullProductStockMachineProductLinkerWrapper(_productStocks, context);

            context.ProductStocks.UpdateRange(newProductStock);
            return await context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">
        ///
        /// </typeparam>
        /// <param name="oldObjects">
        ///
        /// </param>
        /// <param name="newObjects">
        ///
        /// </param>
        /// <returns></returns>
        private static List<T> CheckListLengthDbObjects<T>(List<T> oldObjects, List<T> newObjects)
        {
            if (newObjects.Count < oldObjects.Count)
                oldObjects.RemoveRange(newObjects.Count, (oldObjects.Count - newObjects.Count));
            else if (newObjects.Count > oldObjects.Count)
            {
                newObjects.RemoveRange(0, oldObjects.Count);
                oldObjects.AddRange(newObjects);
            }

            return oldObjects;
        }

        /// <summary>
        /// AsyncDbFullProductStockMachineProductLinkerWrapper exists to asynchronously transform a list of Task DbInitializer._ProductStocks into a Task&lt;List&lt;ProductStock&gt;&gt;.
        /// <para>The primary purpose of this function is to be used in the TwonkUpdate function so that it can asynchronously update the required fields in ProductStock.</para>
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
        private static async Task<List<ProductStock>> AsyncDbFullProductStockMachineProductLinkerWrapper(List<Task<DbInitializer._ProductStock>> newList_ProductStock, LocationContext context)
        {
            var _context = context;

            // Get all the new ProductStock from the API in the correct formatting.
            var newProductStocks = await AsyncDbProductStockMachineProductLinker(newList_ProductStock, _context);

            // Here the Machines get linked to the oldProductStocks to be used later in the function.
            var oldProductStocks = context.ProductStocks.ToList();
            oldProductStocks = CheckListLengthDbObjects(oldProductStocks, newProductStocks);

            foreach (var oldProductStock in oldProductStocks)
            {
                oldProductStock.Machine = context.Machines.ToList().Find(cm => cm.ID.Equals(oldProductStock.MachineId));
            }

            // Update the complete list of ProductStock with the ones from the updated in newProductStock.
            // Doing so requires finding the index of the oldProductStocks that needs to be update and using that index to update the amount.
            for (var i = 0; i < newProductStocks.Count; i++)
            {
                oldProductStocks[i].ProductId = newProductStocks[i].ProductId;
                oldProductStocks[i].MachineId = newProductStocks[i].MachineId;
                oldProductStocks[i].AvailableCount = newProductStocks[i].AvailableCount;
                oldProductStocks[i].MinCount = newProductStocks[i].MinCount;
                oldProductStocks[i].MaxCount = newProductStocks[i].MaxCount;
                oldProductStocks[i].RefillAdviceCount = newProductStocks[i].RefillAdviceCount;
            }

            // Return the updated list.
            return oldProductStocks;
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
        /// AsyncApiRequester asynchronously requests the VendingWebAPI.
        /// <para>Much like the ApiRequester this function requests the VendingWebAPI in order to get the data required to build the LocationContext Database.</para>
        /// <para>The difference however is that this function works asynchronously.</para>
        /// </summary>
        /// <param name="header">
        /// Divines what part of the API it will request data from.
        /// </param>
        /// <param name="subheader">
        /// Details which part of the API within the header is queried. 
        /// </param>
        /// <returns>
        /// A GET request Task IRestResponse.
        /// </returns>
        private static async Task<IRestResponse> AsyncApiRequester(string header, string subheader)
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
            return await client.ExecuteGetAsync(request);
        }

        /// <summary>
        /// AsyncDbProductStockMachineProductLinker changes the ProductId and MachineId from the List of _ProductStock in order to update them to their correct values.
        /// After having done so AsyncDbProductStockMachineProductLinker returns a Task list of ProductStock with the correct values.
        /// </summary>
        /// <param name="productStocks">
        /// A list of Task _ProductStock with to be updated ProductId and MachineId.
        /// </param>
        /// <param name="context">
        /// Context of the LocationContext Database.
        /// </param>
        /// <returns>
        /// A Task list of ProductStock with correct Ids.
        /// </returns>
        private static async Task<List<ProductStock>> AsyncDbProductStockMachineProductLinker(List<Task<DbInitializer._ProductStock>> productStocks, LocationContext context)
        {
            // Setup two variables so that they won't constantly be called while looping over productStocks.
            var products = context.Products.ToList();
            var machines = context.Machines.ToList();

            // Instead of being able to do this in a LINQ function like in the non async version of this function we have to resolve creating the list with a foreach loop instead.
            // This is done by looping over the productStocks and from those awaiting their result (as they were created asynchronously).
            // After this they add the items to the list with the correct ProductId and MachineId.
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
            
            // Return the correct list of ProductStock.
            return list;
        }
    }
}