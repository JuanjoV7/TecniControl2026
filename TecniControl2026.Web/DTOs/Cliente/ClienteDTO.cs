namespace TecniControl2026.Web.DTOs.Cliente
{
    public class ClienteDTO
    {
        public Guid Id { get; set; }
        public required string Documento { get; set; }
        public required string Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; } = true;
    }
}
