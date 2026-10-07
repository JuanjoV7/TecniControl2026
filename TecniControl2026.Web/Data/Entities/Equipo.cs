using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.Data.Entities
{
    public class Equipo : IId
    {
        [Key]
        public Guid Id { get; set; }

        public TipoEquipo Tipo { get; set; }

        [MaxLength(50)]
        public required string Marca { get; set; }

        [MaxLength(100)]
        public required string Modelo { get; set; }

        [MaxLength(100)]
        public string? Serial { get; set; }

        public Guid ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public ICollection<OrdenServicio> OrdenesServicio { get; set; } = new List<OrdenServicio>();
    }
}
