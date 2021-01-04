using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Data
{
    public class DetailsModel : PageModel
    {
        private readonly SDE_AE_VoorraadApp.Data.LocationContext _context;

        public DetailsModel(SDE_AE_VoorraadApp.Data.LocationContext context)
        {
            _context = context;
        }

        public Location Location { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Location = await _context.Locations.FirstOrDefaultAsync(m => m.ID == id);

            if (Location == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
