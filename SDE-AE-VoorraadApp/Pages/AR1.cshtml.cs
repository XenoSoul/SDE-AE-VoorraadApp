using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Pages
{
    [Authorize]
    public class AR1Model : PageModel
    {
        private readonly LocationContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AR1Model> _logger;

        public AR1Model(SignInManager<IdentityUser> signInManager, ILogger<AR1Model> logger, LocationContext context)
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        public List<ListRequester.DateTimeOrderList> OrderLocationJoin { get; private set; }
        public void OnGet()
        {
            OrderLocationJoin = ListRequester.RequestDateOrderLists(_context);
        }

        public IActionResult OnPostIndex()
        {
            return RedirectToPage("Index");
        }
    }
}
