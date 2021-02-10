using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Data
{
    /// <summary>
    /// The ListRequester Class is the main entry point for the creation of, and getting of, any OrderList and associated Orders.
    /// </summary>
    public static class ListRequester
    {
        /// <summary>
        /// CreateList creates a OrderList entry in the OrderList table with Orders for all the missing products of the locations provided.
        /// </summary>
        /// <param name="context">
        /// Context of the LocationContext Database.
        /// </param>
        /// <param name="locations">
        /// List of ints representing the indexes of the different locations selected.
        /// </param>
        /// <returns>
        /// An int equal to the number of lines affected by most recent change.
        /// </returns>
        public static async Task<int> CreateList(LocationContext context, IEnumerable<int> locations)
        {
            // For the locations input we find all the locations in which their ID is the same as one of the ints in location.
            var locationsList = locations.Select(location => context.Locations.ToList().Find(l => l.ID == location)).ToList();

            // To find all the machines listed under those locations we make a new list of Machine.
            // We add to that list any machine of which the the LocationID is the same as the ID of the location and their name does not contain the word "Betaalzuil".
            // This is because "Betaalzuil" machines don't have a stock and so could possibly screw with the operation of this function.
            var machineList = new List<Machine>();
            foreach (var location in locationsList)
            {
                machineList.AddRange(context.Machines.ToList().FindAll(m => m.LocationID.Equals(location.ID) && !m.Name.Contains("Betaalzuil")));
            }

            // First we find the most recent OrderList if was created less then 30 minutes ago.
            // This is important because you would not want to create a list with the same products if those items have yet to be delivered.
            // As such, when finding the different ProductStocks to turn into orders the amount is subtracted equal to the amount on the orderlist created less then 30 minutes ago.
            var sa = context.OrderLists.ToList()
                .Find(ol => DateTime.Now.Subtract(ol.DateTimeCreated) <  TimeSpan.FromMinutes(30));
            var productStockList = new List<ProductStock>();
            foreach (var machine in machineList)
            {
                productStockList.AddRange(context.ProductStocks.ToList().FindAll(ps =>
                    ps.MachineId == machine.ID && ps.RefillAdviceCount -
                    (sa != null && context.Orders.ToList().Find(o => o.ProductStockID == ps.ID) != null
                        ? context.Orders.ToList().Find(o => o.ProductStockID.Equals(ps.ID)).SnapRefillAdviceCount
                        : 0) > 0));
            }

            // Check if the total amount of ProductStock to be turned into orders is 0.
            // If so the rest of this function does not need to be executed as there would be nothing to save.
            // As such we return to where the function was called.
            if (productStockList.Count == 0)
                return productStockList.Count;

            // If productStockList.Count != 0 a new entry in the OrderList table gets created with the current DateTime as well as a name of which locations are included in the list.
            // After which the changes get saved.
            await context.OrderLists.AddAsync(new OrderList
            {
                DateTimeCreated = DateTime.Now,
                Name = $"{DateTime.Now:dddd_dd-MM-yyyy_HH-mm-ss}_Filialen-{string.Join("", locations.ToArray()) }"
            });
            await context.SaveChangesAsync();

            var recentProducts = productStockList.Select(productStock => context.Products.ToList().Find(p => p.ID == productStock.ProductId)).ToList();

            // Being that a OrderList entry has just been added the ID of the latest OrderList gets saved into a variable to be used later.
            // This ID is ofcourse the one we just created.
            // Turning this into a List<Order> the proper values get assigned as well as a priority value.
            // The Priority gets determined by dividing the current RefillAdviceCount by MaxCount.
            // If this value is equal to 1 (that meaning the item is completely empty) then the priority of that item is 10 times as high as it would have been if it was 99% empty.
            var orderId = context.OrderLists.ToList().Last().ID;
            var orderList = productStockList.Select(productStock => new Order
            {
                ProductStockID = productStock.ID,
                ProductName = productStock.Product.Name,
                SnapRefillAdviceCount = productStock.RefillAdviceCount,
                SnapAvailableCount = productStock.AvailableCount,
                SnapMaxCount = productStock.MaxCount,

                Priority = Math.Abs((float)productStock.RefillAdviceCount / productStock.MaxCount * 100f - 100f) < 1
                    ? 1000f
                    : (float)productStock.RefillAdviceCount / productStock.MaxCount * 100f,
                OrderListID = orderId
            }).ToList();

            // The Orders get added to the Order table asynchronously and saved.
            // After which the total amount of rows affected gets returned.
            await context.Orders.AddRangeAsync(orderList.OrderByDescending(ol => ol.ProductName));
            return await context.SaveChangesAsync();
        }

        /// <summary>
        /// RequestRecentList gets the most recent list created in the OrderList table.
        /// </summary>
        /// <param name="context">
        /// context of the LocationContext Database.
        /// </param>
        /// <returns>
        /// The most recent Orders with the accompanying OrderList entry.
        /// </returns>
        public static List<OrderLocationJoin> RequestRecentList(LocationContext context)
        {
            return CreateOrderLocationJoins(
                context.Orders.ToList().FindAll(o => o.OrderListID == context.OrderLists.ToList().Last().ID), context);
        }

        /// <summary>
        /// RequestDateOrderLists orders all the OrderLists in a DateTimeOrderList List of the last 365 days.
        /// </summary>
        /// <param name="date">
        ///
        /// </param>
        /// <param name="context">
        /// Context of the LocationContext Database.
        /// </param>
        /// <returns>
        /// A List of <see cref="DateTimeOrderList"/>.
        /// </returns>
        public static List<OrderList> RequestDateOrderLists(DateTime date, LocationContext context)
        {
            // In order to get the last 365 days of OrderLists there first a list gets created that the OrderLists will be cast into.
            // This list then gets looped over using a for loop 365 (being equal to a year).
            // The current DateTime gets saved minus the amount of days equal to the loop counter.
            // It then finds all the OrderLists associated with that day.
            // Every time a particular day has 0 OrderLists it skips adding it to the list and goes to the next day.

            var dateOrderLists = context.OrderLists.ToList().FindAll(ol =>
                    ol.DateTimeCreated.Date == date.Date)
                .OrderByDescending(ol => ol.DateTimeCreated).ToList();

            // Returns all the Orderlists of the last year in a List<DateTimeOrderList>.
            return dateOrderLists;
        }

        /// <summary>
        /// RequestDateOrderList gets the OrderLocationJoin in accordance with the orderListId input.
        /// </summary>
        /// <param name="context">
        /// Context of the LocationContext Database.
        /// </param>
        /// <param name="orderListId">
        /// Id the of the to be requested OrderList and all associated orders.
        /// </param>
        /// <returns>
        /// A list of <see cref="OrderLocationJoin"/> objects.
        /// </returns>
        public static List<OrderLocationJoin> RequestDateOrderList(LocationContext context, int orderListId)
        {
            return CreateOrderLocationJoins(
                context.Orders.ToList().FindAll(o => o.OrderListID.Equals(orderListId)), context);
        }

        /// <summary>
        /// CreateOrderLocationJoins Joins together A list of Orders with: ProductStock, Machines and Locations
        /// </summary>
        /// <param name="orders">
        /// <see cref="Order"/> to be sorted by location.
        /// </param>
        /// <param name="context">
        /// <see cref="LocationContext"/> used to import parts of the LocationContext Database
        /// </param>
        /// <returns>
        /// A list of <see cref="OrderLocationJoin"/>.
        /// </returns>
        private static List<OrderLocationJoin> CreateOrderLocationJoins(List<Order> orders, LocationContext context)
        {
            // This function links together first ProductStock with Orders, then Machines with ProductStock, and then finally Products with ProductStock.
            // This is done in order to assure that every data type is linked to one another to be returned.
            // The product link particularly is there to facilitate the display of the names of the products from Productstock, wherever the user might need it.
            var recentProductStocks = orders.Select(order => context.ProductStocks.ToList().Find(ps => ps.ID == order.ProductStockID)).ToList();
            var recentMachines = recentProductStocks.Select(productStock => context.Machines.ToList().Find(m => m.ID == productStock.MachineId)).ToList();
            var recentProducts = recentProductStocks.Select(productStock => context.Products.ToList().Find(p => p.ID == productStock.ProductId)).ToList();

            // After these links have been established the established links get used in LINQ function that creates a
            // OrderLocationJoin object for every location that might be included in the list of orders.
            return (from location in context.Locations.ToList()
                let allOrdersLocation = orders.FindAll(o => o.ProductStock.Machine.LocationID == location.ID)
                where allOrdersLocation.Count != 0
                let allOrdersPriority = allOrdersLocation.Sum(o => o.Priority)
                select new OrderLocationJoin { Orders = allOrdersLocation, Location = location, Priority = allOrdersPriority }).ToList();
        }

        /// <summary>
        /// Is used to display a long list of <see cref="OrderList"/> sorted by <see cref="DateTime"/> created.
        /// </summary>
        public class DateTimeOrderList
        {
            public DateTime Day { get; set; }
            public List<OrderList> OrderLists { get; set; }
        }

        /// <summary>
        /// An object containing the combined priority of the Orders within.
        /// </summary>
        public class OrderLocationJoin
        {
            public Location Location { get; set; }
            public List<Order> Orders { get; set; }
            // Contains a combined priority of all order included in Orders.
            // This is done to determine which location has priority over the other.
            public float Priority { get; set; }
        }
    }
}