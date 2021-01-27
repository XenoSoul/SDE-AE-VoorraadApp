using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;

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

        public List<ListRequester.DateTimeOrderList> OrderLocationJoin { get; private set; }

        [BindProperty]
        public int OrderListIDAR1 { get; set; }

        /// <summary>
        /// Gets all the <see cref="ListRequester.DateTimeOrderList"/> vales and cast them into <see cref="OrderLocationJoin"/>.
        /// </summary>
        public void OnGet()
        {
            OrderLocationJoin = ListRequester.RequestDateOrderLists(_context);
        }

        /// <summary>
        /// Sends redirect request for "AR1_1" with the RouteValue equal of the selected OrderList's ID.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult OnPost()
        {
            return RedirectToPage("AR1_1", new { Action = OrderListIDAR1 });
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
