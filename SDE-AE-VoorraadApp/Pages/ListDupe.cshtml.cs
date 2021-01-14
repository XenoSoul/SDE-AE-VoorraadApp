using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SDE_AE_VoorraadApp.Pages
{
    [Authorize]
    public class ListDupeModel : PageModel
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
