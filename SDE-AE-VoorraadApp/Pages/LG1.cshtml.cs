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

        public async Task OnGetAsync()
        {
            var result = await DbUpdater.TwinkUpdate(_context);
            if (result > 0)
            {
                _logger.LogInformation($"{result} lines were updated!!!");
            }
            Location = await _context.Locations.ToListAsync();
        }

        public async Task<IActionResult> OnPostLG1_1()
        {
            var rad = await ListRequester.CreateList(_context, LocationToPrint.Contains(0) ? _context.Locations.ToList().Select(l => l.ID): LocationToPrint);
            return RedirectToPage(rad > 0 ? "LG1_1" : "ListDupe");
        }
    }
}