using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SDE_AE_VoorraadApp.Pages
{
    public class LG1_1Model : PageModel
    {
        public void OnGet()
        {

        }

        public IActionResult OnPostIndex()
        {
            return RedirectToPage("Index");
        }
    }
}