using System.Collections.Generic;
using System.Linq;
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
    public class LG1_1Model : PageModel
    {
        private readonly LocationContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LG1_1Model> _logger;

        public LG1_1Model(SignInManager<IdentityUser> signInManager, ILogger<LG1_1Model> logger, LocationContext context)
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        public OrderList Orders { get; set; }
        public List<ListRequester.OrderLocationJoin> OrderLocationJoin { get; private set; }
        public List<ListRequester.OrderLocationJoin> LocationPriority { get; private set; }

        public void OnGet()
        {
            var orderLocation = ListRequester.RequestRecentList(_context);
            OrderLocationJoin = orderLocation;
            LocationPriority = orderLocation.OrderByDescending(ol => ol.Priority).ToList();
            Orders = _context.OrderLists.ToList().Last();
        }

        public IActionResult OnPostIndex()
        {
            return RedirectToPage("Index");
        }
    }
}