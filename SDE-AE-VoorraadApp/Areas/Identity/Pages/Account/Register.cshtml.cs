using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SDE_AE_VoorraadApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        public IActionResult OnGet()
        {
            return LocalRedirect("~/Login");
        }

        public IActionResult OnPost()
        {
            return LocalRedirect("~/Login");
        }
    }
}
