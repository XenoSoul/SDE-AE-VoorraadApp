using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SDE_AE_VoorraadApp.Data;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Pages
{
    public class LG1_1Model : PageModel
    {
        private readonly LocationContext _context;

        public LG1_1Model(LocationContext context)
        {
            _context = context;
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