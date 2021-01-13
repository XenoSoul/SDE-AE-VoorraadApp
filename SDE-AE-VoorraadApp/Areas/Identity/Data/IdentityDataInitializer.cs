using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SDE_AE_VoorraadApp.Data
{
    public class IdentityDataInitializer
    {
        public static void SeedUsers(UserManager<IdentityUser> userManager)
        {
            if (userManager.FindByEmailAsync("info@appeltje-eitje-automaten.nl").Result != null) return;
            var user = new IdentityUser()
            {
                UserName = "info@appeltje-eitje-automaten.nl",
                NormalizedUserName = "info@appeltje-eitje-automaten.nl",
                Email = "info@appeltje-eitje-automaten.nl",
                NormalizedEmail = "info@appeltje-eitje-automaten.nl",
                EmailConfirmed = true
            };

            var result = userManager.CreateAsync(user, "$Tr0NgPa55WorD!").Result;
            if (!result.Succeeded)
            {
                Console.WriteLine("Something went Wrong");
            }
        }
    }
}