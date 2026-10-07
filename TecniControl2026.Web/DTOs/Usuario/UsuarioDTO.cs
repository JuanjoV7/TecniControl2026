namespace TecniControl2026.Web.DTOs.Usuario
{
    public class UsuarioDTO
    {
        public Guid Id { get; set; }
        public string Documento { get; set; } = null!;
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool Activo { get; set; }
        public Guid RolId { get; set; }
        public string RolNombre { get; set; } = null!;
    }
}
