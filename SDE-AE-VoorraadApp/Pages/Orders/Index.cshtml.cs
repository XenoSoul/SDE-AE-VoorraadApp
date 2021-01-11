using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Data
{
    public class IndexModel : PageModel
    {
        private readonly SDE_AE_VoorraadApp.Data.LocationContext _context;

        public IndexModel(SDE_AE_VoorraadApp.Data.LocationContext context, ILogger<IndexModel> logger)
        {
            _context = context;
        }

        public IList<Location> Location { get; set; }

        public async Task OnGetAsync()
        {
            var result = await DbUpdater.TwinkUpdate(_context);
            if (result > 0)
            {
                Console.WriteLine($"{result} lines were updated!!!");
            }
            Location = await _context.Locations.ToListAsync();
        }
    }
}
