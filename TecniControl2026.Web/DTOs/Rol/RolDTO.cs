namespace TecniControl2026.Web.DTOs.Rol
{
    public class RolDTO
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public int CantidadUsuarios { get; set; }
        public List<PermisoDTO> Permisos { get; set; } = new();
    }
}
