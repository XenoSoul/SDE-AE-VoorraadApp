using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Data
{
    public static class ListRequester
    {
        public static async Task<List<Order>> CreateList(LocationContext context, List<int> locations)
        {
            context.OrderLists.Add(new OrderList {DateTimeCreated = DateTime.Now, Name = $"{DateTime.Now}-Something"});
            await context.SaveChangesAsync();
            var orderId = context.OrderLists.Last().ID;

            var locationsList = locations.Select(location => context.Locations.ToList().Find(l => l.ID == location)).ToList();
            var MachineList = locationsList.Select(location => context.Machines.ToList().Find(m => m.LocationID == location.ID)).ToList();
            var productStockList = new List<ProductStock>();
            foreach (var machine in MachineList)
            {
                productStockList.AddRange(context.ProductStocks.ToList().FindAll(ps => ps.MachineId == machine.ID));
            }
            var orderList = productStockList.Select(productStock => new Order {ProductStockID = productStock.ID, Quantity = productStock.RefillAdviceCount, OrderID = orderId}).ToList();
            await context.Orders.AddRangeAsync(orderList);

            return orderList;
        }
    }
}