using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.OrdenServicio
{
    // Fila del listado de órdenes
    public class OrdenServicioDTO
    {
        public Guid Id { get; set; }
        public int Numero { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public EstadoOrden Estado { get; set; }
        public string FallaReportada { get; set; } = null!;

        public Guid EquipoId { get; set; }
        public string EquipoDescripcion { get; set; } = null!;

        public Guid ClienteId { get; set; }
        public string ClienteNombre { get; set; } = null!;

        public Guid? TecnicoId { get; set; }
        public string? TecnicoNombre { get; set; }
    }
}
