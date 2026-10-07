using Microsoft.AspNetCore.Authorization;

namespace TecniControl2026.Web.Core.Authorization
{
    // Uso: [TienePermiso(PermisosCatalogo.Clientes.Crear)]
    public class TienePermisoAttribute : AuthorizeAttribute
    {
        public const string PolicyPrefix = "Permiso:";

        public TienePermisoAttribute(string permiso)
        {
            Policy = PolicyPrefix + permiso;
        }
    }
}
