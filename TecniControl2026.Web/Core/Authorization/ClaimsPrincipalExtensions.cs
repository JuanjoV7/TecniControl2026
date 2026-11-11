using System.Security.Claims;

namespace TecniControl2026.Web.Core.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
        public const string FullNameClaim = "FullName";
        public const string RoleClaim = "Role";

        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            string? value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out Guid id) ? id : null;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(FullNameClaim) ?? user.Identity?.Name ?? string.Empty;
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(RoleClaim) ?? string.Empty;
        }
    }
}
