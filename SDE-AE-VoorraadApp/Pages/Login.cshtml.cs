using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SDE_AE_VoorraadApp.Pages
{
    public class LoginModel : PageModel
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