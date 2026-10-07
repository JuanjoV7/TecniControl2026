using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.Data.Entities
{
    public class OrdenServicio : IId
    {
        [Key]
        public Guid Id { get; set; }

        // Consecutivo visible para el cliente, generado por la base de datos
        public int Numero { get; set; }

        public Guid EquipoId { get; set; }
        public Equipo Equipo { get; set; } = null!;

        // Recepción
        public DateTime FechaRecepcion { get; set; }

        public Guid RecibidaPorId { get; set; }
        public Usuario RecibidaPor { get; set; } = null!;

        [MaxLength(500)]
        public required string FallaReportada { get; set; }

        [MaxLength(500)]
        public string? Accesorios { get; set; }

        // Atención técnica
        public Guid? TecnicoId { get; set; }
        public Usuario? Tecnico { get; set; }

        [MaxLength(2000)]
        public string? Diagnostico { get; set; }

        [MaxLength(2000)]
        public string? TrabajoRealizado { get; set; }

        public EstadoOrden Estado { get; set; } = EstadoOrden.Recibida;

        // Cierre
        public DateTime? FechaEntrega { get; set; }

        public DateTime? FechaCancelacion { get; set; }

        [MaxLength(500)]
        public string? MotivoCancelacion { get; set; }

        public ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();
    }
}
