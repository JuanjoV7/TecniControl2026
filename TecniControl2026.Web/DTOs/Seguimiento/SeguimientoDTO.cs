using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.Seguimiento
{
    public class SeguimientoDTO
    {
        public Guid Id { get; set; }
        public Guid OrdenServicioId { get; set; }
        public DateTime Fecha { get; set; }
        public string Observacion { get; set; } = null!;
        public string UsuarioNombre { get; set; } = null!;
        public EstadoOrden? EstadoAnterior { get; set; }
        public EstadoOrden? EstadoNuevo { get; set; }
    }
}
