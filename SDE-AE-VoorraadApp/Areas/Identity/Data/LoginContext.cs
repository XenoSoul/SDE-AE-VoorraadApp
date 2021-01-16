using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SDE_AE_VoorraadApp.Data
{
    public class LoginContext : IdentityDbContext<IdentityUser>
    {
        public LoginContext(DbContextOptions<LoginContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var hasher = new PasswordHasher<IdentityUser>();
            builder.Entity<IdentityUser>().HasData(new IdentityUser
            {
                Id = "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                UserName = "info@appeltje-eitje-automaten.nl",
                NormalizedUserName = "info@appeltje-eitje-automaten.nl",
                Email = "info@appeltje-eitje-automaten.nl",
                NormalizedEmail = "info@appeltje-eitje-automaten.nl",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "$Tr0NgPa55WorD!"),
                SecurityStamp = string.Empty
            });
        }
    }
}
