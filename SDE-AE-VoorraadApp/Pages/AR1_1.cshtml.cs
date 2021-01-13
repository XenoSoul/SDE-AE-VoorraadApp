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
    public class AR1_1Model : PageModel
    {
        private readonly LocationContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AR1_1Model> _logger;

        public AR1_1Model(SignInManager<IdentityUser> signInManager, ILogger<AR1_1Model> logger, LocationContext context)
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
            var orderLocation = ListRequester.RequestDateOrderList(_context, 2);
            OrderLocationJoin = orderLocation;
            LocationPriority = orderLocation.OrderByDescending(ol => ol.Priority).ToList();
            Orders = _context.OrderLists.ToList().Find(ol => ol.ID.Equals(2));
        }

        public IActionResult OnPostIndex()
        {
            return RedirectToPage("Index");
        }
    }
}