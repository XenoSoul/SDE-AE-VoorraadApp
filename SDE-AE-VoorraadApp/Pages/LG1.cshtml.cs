using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Pages
{
    /// <summary>
    /// Main menu handler for creating an OrderList.
    /// </summary>
    [Authorize]
    public class LG1Model : PageModel
    {
        private readonly LocationContext _context;
        private readonly ILogger<LG1Model> _logger;

        public LG1Model(LocationContext context, ILogger<LG1Model> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public List<int> LocationToPrint { get; set; }

        public IList<Location> Location { get; set; }

        /// <summary>
        /// Fires a <see cref="DbUpdater.TwinkUpdate"/> and loads all locations in the <see cref="LocationContext"/> Database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        public async Task OnGetAsync()
        {
            var result = await DbUpdater.TwinkUpdate(_context);
            if (result > 0)
            {
                _logger.LogInformation($"{result} lines were updated!!!");
            }
            Location = await _context.Locations.ToListAsync();
        }

        /// <summary>
        /// Creates the list from the selected <see cref="LocationToPrint"/>.
        /// </summary>
        public async Task<IActionResult> OnPostLG1_1()
        {
            // Being that the select all button has an ID of 0 the function checks if that values is among the list.
            // If so all locations get loading into the ListRequester.CreateList input.
            // After the list has finished creating, if the total amount of lines affect is greater then 0 the list will get show.
            // if this is not the case however it is seen as a duplicate request and get send to the "ListDupe" page.
            var rad = await ListRequester.CreateList(_context, LocationToPrint.Contains(0) ? _context.Locations.ToList().Select(l => l.ID): LocationToPrint);
            return RedirectToPage(rad > 0 ? "LG1_1" : "ListDupe");
        }
    }
}