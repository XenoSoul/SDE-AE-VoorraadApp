using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Pages
{
    public class LG1Model : PageModel
    {
        private readonly SDE_AE_VoorraadApp.Data.LocationContext _context;
        public LG1Model(SDE_AE_VoorraadApp.Data.LocationContext context)
        {
            _context = context;
        }

        public IList<Location> Location { get; set; }

        public async Task OnGetAsync()
        {
            Location = await _context.Locations.ToListAsync();
        }

        public IActionResult OnPostLG1_1()
        {
            return RedirectToPage("LG1_1");
        }
    }
}