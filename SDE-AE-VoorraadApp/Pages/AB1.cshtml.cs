using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SDE_AE_VoorraadApp.Data;

namespace SDE_AE_VoorraadApp.Pages
{
    /// <summary>
    /// The AB1Model handler is a handler that was crated as a dummy item for something that we intended to implement if we had the time for it
    /// Being that we never did, this never came up.
    /// </summary>
    public class AB1Model : PageModel
    {
        private readonly LocationContext _context;
        private readonly ILogger<AB1Model> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AB1Model(SignInManager<IdentityUser> signInManager, ILogger<AB1Model> logger, LocationContext context)
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task OnGet()
        {
            await DbUpdater.TwonkUpdate(_context);
        }

    }
}
