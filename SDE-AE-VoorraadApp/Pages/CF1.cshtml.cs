using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SDE_AE_VoorraadApp.Pages
{
    /// <summary>
    /// The CF1Model handler is a handler that was crated as a dummy item for something that we intended to implement if we had the time for it
    /// Being that we never did, this never came up.
    /// </summary>
    [Authorize]
    public class CF1Model : PageModel
    {
        public void OnGet()
        {
        }
    }
}
