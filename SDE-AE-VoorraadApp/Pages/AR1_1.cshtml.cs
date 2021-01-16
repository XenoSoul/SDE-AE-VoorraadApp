using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Pages
{
    /// <summary>
    /// Display of archived <see cref="OrderList"/>.
    /// Not accessible if not logged in.
    /// </summary>
    [Authorize]
    public class AR1_1Model : PageModel
    {
        /// <summary>
        /// Contains the context of the <see cref="LocationContext"/> Database.
        /// </summary>
        private readonly LocationContext _context;
        private readonly ILogger<AR1_1Model> _logger;

        public AR1_1Model(ILogger<AR1_1Model> logger, LocationContext context)
        {
            _context = context;
            _logger = logger;
        }

        public OrderList Orders { get; set; }
        public List<ListRequester.OrderLocationJoin> OrderLocationJoin { get; private set; }
        public List<ListRequester.OrderLocationJoin> LocationPriority { get; private set; }

        /// <summary>
        /// Receives a string from <see cref="AR1Model.OnPost"/> and uses this to display the correct OrderList.
        /// </summary>
        /// <param name="action">
        /// The value passed by <see cref="AR1Model.OnPost"/>.
        /// </param>
        public void OnGet(string action)
        {
            // Parse the string value into an int.
            var orderListId = int.Parse(action);
            _logger.LogInformation($"{orderListId} was successfully created");

            // Use the orderListId in order to get the full list.
            // Create a new list by sorting them by Priority.
            var orderLocation = ListRequester.RequestDateOrderList(_context, orderListId);
            _logger.LogInformation($"{orderLocation} was successfully created");

            OrderLocationJoin = orderLocation;
            LocationPriority = orderLocation.OrderByDescending(ol => ol.Priority).ToList();
            Orders = _context.OrderLists.ToList().Find(ol => ol.ID.Equals(orderListId));
        }

        /// <summary>
        /// Redirects the user to the Index page.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult OnPostIndex()
        {
            return RedirectToPage("Index");
        }
    }
}