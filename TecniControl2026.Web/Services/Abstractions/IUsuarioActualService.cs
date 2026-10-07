namespace TecniControl2026.Web.Services.Abstractions
{
    // Datos del usuario que hizo la solicitud HTTP actual
    public interface IUsuarioActualService
    {
        public Guid? UsuarioId { get; }
        public Task<bool> TienePermisoAsync(string permiso);
    }
}
