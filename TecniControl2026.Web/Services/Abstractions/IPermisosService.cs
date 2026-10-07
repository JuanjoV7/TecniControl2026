namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IPermisosService
    {
        public Task<bool> UsuarioTienePermisoAsync(Guid usuarioId, string permiso);
        public Task<HashSet<string>> GetPermisosUsuarioAsync(Guid usuarioId);
    }
}
