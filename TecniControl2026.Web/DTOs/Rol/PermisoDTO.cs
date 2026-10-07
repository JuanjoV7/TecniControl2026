namespace TecniControl2026.Web.DTOs.Rol
{
    public class PermisoDTO
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Modulo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
    }
}
