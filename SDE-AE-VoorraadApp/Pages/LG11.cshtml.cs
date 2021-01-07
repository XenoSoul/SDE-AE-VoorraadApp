using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SDE_AE_VoorraadApp.Models;

namespace SDE___Appeltje_Eitje_Automaten___POC_2.Pages
{
    public class LG11Model : PageModel
    {
        private readonly SDE_AE_VoorraadApp.Data.LocationContext _context;
        public LG11Model(SDE_AE_VoorraadApp.Data.LocationContext context)
        {
            _context = context;
        }

        public IList<Location> Location { get; set; }

        public async Task OnGetAsync()
        {
            Location = await _context.Locations.ToListAsync();
        }
    }
}


