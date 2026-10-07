using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.Data.Entities
{
    public class Seguimiento : IId
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrdenServicioId { get; set; }
        public OrdenServicio OrdenServicio { get; set; } = null!;

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public DateTime Fecha { get; set; }

        [MaxLength(1000)]
        public required string Observacion { get; set; }

        // Se llenan cuando la actuación cambió el estado de la orden
        public EstadoOrden? EstadoAnterior { get; set; }
        public EstadoOrden? EstadoNuevo { get; set; }
    }
}
