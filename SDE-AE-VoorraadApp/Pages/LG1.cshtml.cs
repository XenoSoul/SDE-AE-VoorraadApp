using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Pages
{
    public class LG1Model : PageModel
    {
        private readonly LocationContext _context;

        public LG1Model(LocationContext context)
        {
            _context = context;
        }

        [BindProperty]
        public List<int> LocationToPrint { get; set; }

        public IList<Location> Location { get; set; }

        public async Task OnGetAsync()
        {
            var result = await DbUpdater.TwinkUpdate(_context);
            if (result > 0)
            {
                // TODO: Update Console.WriteLine to something in debug output
                Console.WriteLine($"{result} lines were updated!!!");
            }
            Location = await _context.Locations.ToListAsync();
        }

        // TODO: Change the redirect to something that indicates to the user that location is already fully stocked
        public async Task<IActionResult> OnPostLG1_1()
        {
            // TODO: Select all option functionality
            var rad = await ListRequester.CreateList(_context, LocationToPrint.Contains(0) ? _context.Locations.ToList().Select(l => l.ID): LocationToPrint);
            return RedirectToPage(rad > 0 ? "LG1_1" : "Index");
        }
    }
}