using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Data
{
    public static class ListRequester
    {
        private static int OrderId { get; set; }

        public static async Task<int> CreateList(LocationContext context, IEnumerable<int> locations)
        {
            var locationsList = locations.Select(location => context.Locations.ToList().Find(l => l.ID == location)).ToList();
            var machineList = new List<Machine>();
            foreach (var location in locationsList)
            {
                machineList.AddRange(context.Machines.ToList().FindAll(m => m.LocationID == location.ID && !m.Name.Contains("Betaalzuil")));
            }
            var productStockList = new List<ProductStock>();
            foreach (var machine in machineList)
            {
                productStockList.AddRange(context.ProductStocks.ToList().FindAll(ps => ps.MachineId == machine.ID && ps.RefillAdviceCount > 0));
            }

            if (productStockList.Count == 0)
                return productStockList.Count;

            await context.OrderLists.AddAsync(new OrderList { DateTimeCreated = DateTime.Now, Name = $"{DateTime.Now}-Something" });
            await context.SaveChangesAsync();
            OrderId = context.OrderLists.ToList().Last().ID;
            var orderList = productStockList.Select(productStock => new Order { ProductStockID = productStock.ID, Quantity = productStock.RefillAdviceCount, OrderListID = OrderId }).ToList();
            await context.Orders.AddRangeAsync(orderList);
            return await context.SaveChangesAsync();
        }

        public static async Task<List<OrderLocationJoin>> RequestRecentList(LocationContext context)
        {
            var recentOrders = context.Orders.ToList().FindAll(o => o.OrderListID == context.OrderLists.ToList().Last().ID);
            var recentProductStocks = recentOrders.Select(order => context.ProductStocks.ToList().Find(ps => ps.ID == order.ProductStockID)).ToList();
            var recentMachines = recentProductStocks.Select(productStock => context.Machines.ToList().Find(m => m.ID == productStock.MachineId)).ToList();
            var recentProducts = recentProductStocks.Select(productStock => context.Products.ToList().Find(p => p.ID == productStock.ProductId)).ToList();
            return (from location in context.Locations.ToList()
                let allOrdersLocation = recentOrders.FindAll(o => o.ProductStock.Machine.LocationID == location.ID)
                where allOrdersLocation.Count != 0
                select new OrderLocationJoin {Orders = allOrdersLocation, Location = location}).ToList();
        }

        public static async Task<List<OrderList>> RequestDateOrderLists(LocationContext context, DateTime date)
        {
            return context.OrderLists.ToList().FindAll(ol => ol.DateTimeCreated.Date == date.Date);
        }
        public static async Task<List<Order>> RequestDateOrderList(LocationContext context, int orderListId)
        {
            return context.Orders.ToList().FindAll(o => o.OrderListID == orderListId);
        }

        public class OrderLocationJoin
        {
            public List<Order> Orders { get; set; }
            public Location Location { get; set; }
        }
    }
}