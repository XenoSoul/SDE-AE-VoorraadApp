using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDE___Appeltje_Eitje_Automaten___POC_2.Pages
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
        public IActionResult OnPostLG11()
        {
            return RedirectToPage("LG11");
        }

        public IActionResult OnPostAR11()
        {
            return RedirectToPage("AR11");
        }

        public IActionResult OnPostAB11()
        {
            return RedirectToPage("AB11");
        }

        public IActionResult OnPostCF11()
        {
            return RedirectToPage("CF11");
        }
    }
}
