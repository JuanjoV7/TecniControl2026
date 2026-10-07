using TecniControl2026.Web.DTOs.Seguimiento;

namespace TecniControl2026.Web.DTOs.OrdenServicio
{
    public class OrdenServicioDetalleDTO : OrdenServicioDTO
    {
        public string? ClienteTelefono { get; set; }
        public string RecibidaPorNombre { get; set; } = null!;
        public string? Accesorios { get; set; }
        public string? Diagnostico { get; set; }
        public string? TrabajoRealizado { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public string? MotivoCancelacion { get; set; }

        public List<SeguimientoDTO> Seguimientos { get; set; } = new();
    }
}
