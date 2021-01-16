using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Pages
{
    /// <summary>
    /// Display of most recently created <see cref="OrderList"/>.
    /// Not accessible if not logged in.
    /// </summary>
    [Authorize]
    public class LG1_1Model : PageModel
    {
        /// <summary>
        /// Contains the context of the <see cref="LocationContext"/> Database.
        /// </summary>
        private readonly LocationContext _context;
        private readonly ILogger<LG1_1Model> _logger;

        public LG1_1Model(ILogger<LG1_1Model> logger, LocationContext context)
        {
            _context = context;
            _logger = logger;
        }

        public OrderList Orders { get; set; }
        public List<ListRequester.OrderLocationJoin> OrderLocationJoin { get; private set; }
        public List<ListRequester.OrderLocationJoin> LocationPriority { get; private set; }

        /// <summary>
        /// Gets the most recent <see cref="OrderList"/> created and loads that in the appropriate variables.
        /// </summary>
        public void OnGet()
        {
            var orderLocation = ListRequester.RequestRecentList(_context);
            OrderLocationJoin = orderLocation;
            LocationPriority = orderLocation.OrderByDescending(ol => ol.Priority).ToList();
            Orders = _context.OrderLists.ToList().Last();
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