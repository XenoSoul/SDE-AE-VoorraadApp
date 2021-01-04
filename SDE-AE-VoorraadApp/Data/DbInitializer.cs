using System;
using System.Linq;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(LocationContext context)
        {
            context.Database.EnsureCreated();

            if (context.Locations.Any())
                return;

            // TODO: Create API in order to fill the Location table with data
            var locations = new Location[]
            {

            };

            // TODO: Create API in order to fill the Category table with data
            var categories = new Category[]
            {
                
            };

            // TODO: Create API in order to fill the Product table with data
            var products = new Product[]
            {

            };

            // TODO: Create API in order to fill the Machine table with data
            var machines = new Machine[]
            {

            };

            // TODO: Create API in order to fill the ProductStock table with data
            var productStocks = new ProductStock[]
            {

            };

            // TODO: Create API in order to fill the OrderList table with data
            var orderLists = new OrderList[]
            {

            };

            // TODO: Create API in order to fill the Order table with data
            var orders = new Order[]
            {

            };

        }
    }
}