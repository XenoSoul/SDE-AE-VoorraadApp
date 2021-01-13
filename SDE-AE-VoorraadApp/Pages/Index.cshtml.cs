using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SDE_AE_VoorraadApp.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(SignInManager<IdentityUser> signInManager, ILogger<IndexModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public void OnGet()
        {
        }
        public IActionResult OnPostLG1()
        {
            return RedirectToPage("LG1");
        }

        public IActionResult OnPostAR1()
        {
            return RedirectToPage("AR1");
        }

        public IActionResult OnPostAB1()
        {
            return RedirectToPage("AB1");
        }

        public IActionResult OnPostCF1()
        {
            return RedirectToPage("CF1");
        }

        public async Task<IActionResult> OnPostLogout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return LocalRedirect("~/Login");
        }
    }
}