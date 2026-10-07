using Microsoft.AspNetCore.Authorization;

namespace TecniControl2026.Web.Core.Authorization
{
    public class PermisoRequirement : IAuthorizationRequirement
    {
        public string Permiso { get; }

        public PermisoRequirement(string permiso)
        {
            Permiso = permiso;
        }
    }
}
