using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;

namespace SDE_AE_VoorraadApp.Pages
{
    /// <summary>
    /// Handler of the index page.
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly LocationContext _context;


        public IndexModel(SignInManager<IdentityUser> signInManager, ILogger<IndexModel> logger, LocationContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {

        }

        /// <summary>
        /// A handler to redirect the user to the LG1 page.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult OnPostLG1()
        {
            return RedirectToPage("LG1");
        }

        /// <summary>
        /// A handler to redirect the user to the AR1 page.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult OnPostAR1()
        {
            return RedirectToPage("AR1");
        }

        /// <summary>
        /// A handler to redirect the user to the AB1 page.
        /// The handler does currently work but is not actively in use.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult OnPostAB1()
        {
            return RedirectToPage("AB1");
        }

        /// <summary>
        /// A handler to redirect the user to the CF1 page.
        /// The handler does currently work but is not actively in use.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public IActionResult OnPostCF1()
        {
            return RedirectToPage("CF1");
        }

        /// <summary>
        /// Custom logout function that signs out the user asynchronous and redirects them to the Login screen.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>.
        /// </returns>
        public async Task<IActionResult> OnPostLogout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return LocalRedirect("~/Login");
        }
        
        public async Task<IActionResult> OnPostTwonkReload()
        {
            await DbUpdater.TwonkUpdate(_context);
            return LocalRedirect("~/Index");
        }
    }
}