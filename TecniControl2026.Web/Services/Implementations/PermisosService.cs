using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    // Scoped: los permisos de cada usuario se consultan una sola vez por solicitud
    public class PermisosService : IPermisosService
    {
        private readonly DataContext _context;
        private readonly Dictionary<Guid, HashSet<string>> _cache = new();

        public PermisosService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> UsuarioTienePermisoAsync(Guid usuarioId, string permiso)
        {
            HashSet<string> permisos = await GetPermisosUsuarioAsync(usuarioId);

            return permisos.Contains(permiso);
        }

        public async Task<HashSet<string>> GetPermisosUsuarioAsync(Guid usuarioId)
        {
            if (_cache.TryGetValue(usuarioId, out HashSet<string>? permisos))
            {
                return permisos;
            }

            // Un usuario desactivado no tiene permisos
            List<string> codigos = await _context.Users.Where(u => u.Id == usuarioId && u.Activo)
                                                       .SelectMany(u => u.Rol.RolPermisos)
                                                       .Select(rp => rp.Permiso.Codigo)
                                                       .ToListAsync();

            permisos = new HashSet<string>(codigos);
            _cache[usuarioId] = permisos;

            return permisos;
        }
    }
}
