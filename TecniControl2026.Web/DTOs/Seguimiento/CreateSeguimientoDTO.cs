using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Seguimiento
{
    public class CreateSeguimientoDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid OrdenServicioId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(1000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Observación")]
        public string Observacion { get; set; } = null!;
    }
}
