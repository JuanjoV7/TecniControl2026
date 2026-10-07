using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.OrdenServicio
{
    public class CancelarOrdenDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid OrdenServicioId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(500, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Motivo de cancelación")]
        public string Motivo { get; set; } = null!;
    }
}
