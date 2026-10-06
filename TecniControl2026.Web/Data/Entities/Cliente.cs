using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Data.Entities
{
    public class Cliente : IId
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(30)]
        public required string Documento { get; set; }

        [MaxLength(100)]
        public required string Nombre { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;
    }
}
