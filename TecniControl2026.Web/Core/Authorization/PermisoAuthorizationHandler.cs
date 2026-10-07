using Microsoft.AspNetCore.Authorization;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Core.Authorization
{
    // Consulta los permisos en la base de datos en cada solicitud, de modo que los cambios
    // en roles o la desactivación de un usuario tienen efecto inmediato.
    public class PermisoAuthorizationHandler : AuthorizationHandler<PermisoRequirement>
    {
        private readonly IPermisosService _permisosService;

        public PermisoAuthorizationHandler(IPermisosService permisosService)
        {
            _permisosService = permisosService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermisoRequirement requirement)
        {
            Guid? usuarioId = context.User.GetUsuarioId();

            if (usuarioId is null)
            {
                return;
            }

            if (await _permisosService.UsuarioTienePermisoAsync(usuarioId.Value, requirement.Permiso))
            {
                context.Succeed(requirement);
            }
        }
    }
}
