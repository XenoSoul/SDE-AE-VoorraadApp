using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;
using SDE_AE_VoorraadApp.Models;

namespace SDE_AE_VoorraadApp.Pages
{
    /// <summary>
    /// Archive display model.
    /// Not accessible  if not logged in.
    /// </summary>
    [Authorize]
    public class AR1Model : PageModel
    {
        /// <summary>
        /// Contains the context of the <see cref="LocationContext"/> Database.
        /// </summary>
        private readonly LocationContext _context;
        private readonly ILogger<AR1Model> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AR1Model(SignInManager<IdentityUser> signInManager, ILogger<AR1Model> logger, LocationContext context)
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger;

        }

        public List<OrderList> OrderLocationJoin { get; private set; }

        [BindProperty]
        public DateTime DateTimeRequested { get; set; }

        [BindProperty]
        public int OrderListIDAR1 { get; set; }

        [BindProperty]
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets all the <see cref="ListRequester.DateTimeOrderList"/> vales and cast them into <see cref="OrderLocationJoin"/>.
        /// </summary>
        public void OnGet(string date)
        {
            DateTimeRequested = date == null ? DateTime.Now : DateTime.Parse(date);

            var orderlist= ListRequester.RequestDateOrderLists(DateTimeRequested, _context);

            if (orderlist.Count == 0)
                WarningMessage = "Er zijn geen lijsten op deze dag aangemaakt";

            OrderLocationJoin = orderlist;
        }

        /// <summary>
        /// Sends redirect request for "AR1_1" with the RouteValue equal of the selected OrderList's ID.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult OnPost()
        {
            return RedirectToPage("AR1_1", new { List = OrderListIDAR1 });
        }

        public IActionResult OnPostDetermineDate()
        {
            return RedirectToPage("AR1", new { Date = DateTimeRequested });
        }

        /// <summary>
        /// Redirects the user to the Index page.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult OnPostIndex()
        {
            return RedirectToPage("Index");
        }
    }
}
