using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.OrdenServicio
{
    public class EntregarOrdenDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid OrdenServicioId { get; set; }

        [MaxLength(1000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Observación")]
        public string? Observacion { get; set; }
    }
}
