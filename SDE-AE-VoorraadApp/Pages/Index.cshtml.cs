using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SDE_AE_VoorraadApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
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
    }
}