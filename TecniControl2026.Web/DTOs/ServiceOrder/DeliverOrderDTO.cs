using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.ServiceOrder
{
    public class DeliverOrderDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid ServiceOrderId { get; set; }

        [MaxLength(1000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Observación")]
        public string? Observation { get; set; }
    }
}
