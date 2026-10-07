using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class UsuarioActualService : IUsuarioActualService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermisosService _permisosService;

        public UsuarioActualService(IHttpContextAccessor httpContextAccessor, IPermisosService permisosService)
        {
            _httpContextAccessor = httpContextAccessor;
            _permisosService = permisosService;
        }

        public Guid? UsuarioId => _httpContextAccessor.HttpContext?.User.GetUsuarioId();

        public async Task<bool> TienePermisoAsync(string permiso)
        {
            Guid? usuarioId = UsuarioId;

            return usuarioId is not null && await _permisosService.UsuarioTienePermisoAsync(usuarioId.Value, permiso);
        }
    }
}
