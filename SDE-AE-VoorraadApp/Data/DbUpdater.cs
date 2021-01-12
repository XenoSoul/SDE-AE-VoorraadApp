using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using SDE_AE_VoorraadApp.Models;
using SQLitePCL;

namespace SDE_AE_VoorraadApp.Data
{
    public static class DbUpdater
    {
        public static async Task<int> TwinkUpdate(LocationContext context)
        {
            if (!context.Locations.Any())
            {
                DbInitializer.Initialize(context);
            }

            var _context = context;

            var allMachineIds = context.Machines.Select(m => m.MachineId).ToArray();
            
            var _productStocks = allMachineIds.Select(async machineId =>
                JsonSerializer.Deserialize<DbInitializer._ProductStock>(
                    (await AsyncApiRequester("machines/stock", $"{machineId}")).Content) ??
                throw new InvalidOperationException()).ToList();
            var currentProductStock = await
                 AsyncDbProductStockMachineProductLinkerRevampSymphonyOfTheNight(_productStocks,
                     _context);

            context.ProductStocks.UpdateRange(currentProductStock);
            
            return await context.SaveChangesAsync();
        }

        // TODO: Improve Upon TwonkUpdate to backup OrderList and Orders
        public static void TwonkUpdate(LocationContext context)
        {
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

            DbInitializer.Initialize(context);
        }

        private static async Task<List<ProductStock>> AsyncDbProductStockMachineProductLinkerRevampSymphonyOfTheNight(List<Task<DbInitializer._ProductStock>> newStoks, LocationContext context)
        {
            var _context = context;

            var newStonks = await AsyncDbProductStockMachineProductLinker(newStoks, _context);

            var oldStonks = context.ProductStocks.ToList();
            foreach (var oldStonk in oldStonks)
            {
                oldStonk.Machine = context.Machines.ToList().Find(cm => cm.ID == oldStonk.MachineId);
            }

            foreach (var newStonk in newStonks)
            {
                var currentOldStonkIndex = oldStonks.FindIndex(os =>
                    os.ProductId == newStonk.ProductId && os.MachineId == newStonk.MachineId);

                oldStonks[currentOldStonkIndex].AvailableCount = newStonk.AvailableCount;
                oldStonks[currentOldStonkIndex].RefillAdviceCount = newStonk.RefillAdviceCount;
            }

            return oldStonks;
        }

        private static async Task<IRestResponse> AsyncApiRequester(string header, string subheader)
        {
            var client = new RestClient($"https://api-staging-vendingweb.azurewebsites.net/api/external/{header}/")
            {
                Authenticator = new HttpBasicAuthenticator("AppelEitjeTest", "appeleitje@!78")
            };

            var request = new RestRequest($"{subheader}", DataFormat.Json);

            return await client.ExecuteGetAsync(request);
        }

        private static async Task<List<ProductStock>> AsyncDbProductStockMachineProductLinker(List<Task<DbInitializer._ProductStock>> productStocks, LocationContext context)
        {
            var list = new List<ProductStock>();
            var products = context.Products.ToList();
            var machines = context.Machines.ToList();

            foreach (var result in productStocks.ToList())
            {
                var tResult = await result;
                list.AddRange(tResult.ProductStock.Select(cProductStock => new ProductStock
                {
                    ID = 0,
                    ProductId = products.Find(p => p.ProductId == cProductStock.ProductId).ID,
                    MachineId = machines.Find(x => x.MachineId == tResult.MachineId).ID,
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