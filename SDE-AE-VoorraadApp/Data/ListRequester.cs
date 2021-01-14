using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDE_AE_VoorraadApp.Models;

// TODO: Documentation
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
            var sa = context.OrderLists.ToList()
                .Find(ol => DateTime.Now.Subtract(ol.DateTimeCreated) < new TimeSpan(0, 30, 0));
            var productStockList = new List<ProductStock>();
            foreach (var machine in machineList)
            {
                productStockList.AddRange(context.ProductStocks.ToList().FindAll(ps =>
                    ps.MachineId == machine.ID && ps.RefillAdviceCount -
                    (sa != null && context.Orders.ToList().Find(o => o.ProductStockID == ps.ID) != null
                        ? context.Orders.ToList().Find(o => o.ProductStockID.Equals(ps.ID)).Quantity
                        : 0) > 0));
            }

            if (productStockList.Count == 0)
                return productStockList.Count;

            await context.OrderLists.AddAsync(new OrderList
            {
                DateTimeCreated = DateTime.Now,
                Name = $"{DateTime.Now:dddd_dd-MM-yyyy_HH-mm-ss}_Filialen-{string.Join("", locations.ToArray()) }"
            });
            await context.SaveChangesAsync();

            OrderId = context.OrderLists.ToList().Last().ID;
            var orderList = productStockList.Select(productStock => new Order
            {
                ProductStockID = productStock.ID, Quantity = productStock.RefillAdviceCount,
                Priority = Math.Abs((float)productStock.RefillAdviceCount / productStock.MaxCount * 100f - 100f) < 1
                    ? 1000f
                    : (float)productStock.RefillAdviceCount / productStock.MaxCount * 100f,
                OrderListID = OrderId
            }).ToList();
            await context.Orders.AddRangeAsync(orderList);
            return await context.SaveChangesAsync();
        }


        public static List<OrderLocationJoin> RequestRecentList(LocationContext context)
        {
            return CreateOrderLocationJoins(
                context.Orders.ToList().FindAll(o => o.OrderListID == context.OrderLists.ToList().Last().ID), context);
        }

        public static List<DateTimeOrderList> RequestDateOrderLists(LocationContext context)
        {
            var datetimeOrderLists = new List<DateTimeOrderList>();
            for (int i = 0; i < 365; i++)
            {
                var dateOrderLists = context.OrderLists.ToList().FindAll(ol =>
                    ol.DateTimeCreated.Date == DateTime.Now.Subtract(new TimeSpan(i, 0, 0, 0)).Date);
                if (dateOrderLists.Count == 0)
                    continue;

                datetimeOrderLists.Add(new DateTimeOrderList
                {
                    Day = DateTime.Now.Subtract(new TimeSpan(i, 0, 0, 0)),
                    OrderLists = dateOrderLists
                });
            }

            return datetimeOrderLists;
        }

        public static List<OrderLocationJoin> RequestDateOrderList(LocationContext context, int orderListId)
        {
            return CreateOrderLocationJoins(
                context.Orders.ToList().FindAll(o => o.OrderListID.Equals(orderListId)), context);
        }

        private static List<OrderLocationJoin> CreateOrderLocationJoins(List<Order> orders, LocationContext context)
        {
            var recentProductStocks = orders.Select(order => context.ProductStocks.ToList().Find(ps => ps.ID == order.ProductStockID)).ToList();
            var recentMachines = recentProductStocks.Select(productStock => context.Machines.ToList().Find(m => m.ID == productStock.MachineId)).ToList();
            var recentProducts = recentProductStocks.Select(productStock => context.Products.ToList().Find(p => p.ID == productStock.ProductId)).ToList();
            return (from location in context.Locations.ToList()
                let allOrdersLocation = orders.FindAll(o => o.ProductStock.Machine.LocationID == location.ID)
                where allOrdersLocation.Count != 0
                let allOrdersPriority = allOrdersLocation.Sum(o => o.Priority)
                select new OrderLocationJoin { Orders = allOrdersLocation, Location = location, Priority = allOrdersPriority }).ToList();
        }

        public class DateTimeOrderList
        {
            public DateTime Day { get; set; }
            public List<OrderList> OrderLists { get; set; }
        }

        public class OrderLocationJoin
        {
            public Location Location { get; set; }
            public List<Order> Orders { get; set; }
            public float Priority { get; set; }
        }
    }
}