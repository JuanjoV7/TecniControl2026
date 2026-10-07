using System.Security.Claims;

namespace TecniControl2026.Web.Core.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
        public const string NombreCompletoClaim = "NombreCompleto";
        public const string RolClaim = "Rol";

        public static Guid? GetUsuarioId(this ClaimsPrincipal user)
        {
            string? value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out Guid id) ? id : null;
        }

        public static string GetNombreCompleto(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(NombreCompletoClaim) ?? user.Identity?.Name ?? string.Empty;
        }

        public static string GetRol(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(RolClaim) ?? string.Empty;
        }
    }
}
