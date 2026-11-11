using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.FollowUp
{
    public class CreateFollowUpDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid ServiceOrderId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(1000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Observación")]
        public string Observation { get; set; } = null!;
    }
}
