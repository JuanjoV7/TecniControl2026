using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;

namespace TecniControl2026.Web.Core.Authorization
{
    // Agrega a la cookie el nombre y el rol del usuario para mostrarlos en el layout.
    // Los permisos NO se guardan en la cookie: se validan contra la base de datos.
    public class UsuarioClaimsPrincipalFactory : UserClaimsPrincipalFactory<Usuario>
    {
        private readonly DataContext _context;

        public UsuarioClaimsPrincipalFactory(UserManager<Usuario> userManager, IOptions<IdentityOptions> optionsAccessor, DataContext context)
            : base(userManager, optionsAccessor)
        {
            _context = context;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);

            string? rol = await _context.Roles.Where(r => r.Id == user.RolId)
                                              .Select(r => r.Nombre)
                                              .FirstOrDefaultAsync();

            identity.AddClaim(new Claim(ClaimsPrincipalExtensions.NombreCompletoClaim, user.NombreCompleto));
            identity.AddClaim(new Claim(ClaimsPrincipalExtensions.RolClaim, rol ?? string.Empty));

            return identity;
        }
    }
}
