using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.OrdenServicio
{
    // Lo usa el técnico para documentar diagnóstico, trabajo realizado y avance
    public class RegistrarAtencionDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid OrdenServicioId { get; set; }

        [MaxLength(2000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Diagnóstico")]
        public string? Diagnostico { get; set; }

        [MaxLength(2000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Trabajo realizado")]
        public string? TrabajoRealizado { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Estado")]
        public EstadoOrden Estado { get; set; }

        [MaxLength(1000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Observación")]
        public string? Observacion { get; set; }
    }
}
