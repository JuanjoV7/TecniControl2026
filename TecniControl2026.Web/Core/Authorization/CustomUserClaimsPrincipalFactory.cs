using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Core.Authorization
{
    // Adds the user's name and role to the cookie so they can be shown in the layout.
    // Permissions are NOT stored in the cookie: they are validated against the database.
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
    {
        private readonly DataContext _context;

        public CustomUserClaimsPrincipalFactory(UserManager<User> userManager, IOptions<IdentityOptions> optionsAccessor, DataContext context)
            : base(userManager, optionsAccessor)
        {
            _context = context;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);

            string? role = await _context.Roles.Where(r => r.Id == user.RoleId)
                                              .Select(r => r.Name)
                                              .FirstOrDefaultAsync();

            identity.AddClaim(new Claim(ClaimsPrincipalExtensions.FullNameClaim, user.FullName));
            identity.AddClaim(new Claim(ClaimsPrincipalExtensions.RoleClaim, role ?? string.Empty));

            return identity;
        }
    }
}
