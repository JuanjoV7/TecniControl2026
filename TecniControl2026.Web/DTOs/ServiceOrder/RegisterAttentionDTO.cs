using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.ServiceOrder
{
    // Used by the technician to document diagnosis, work performed and progress
    public class RegisterAttentionDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid ServiceOrderId { get; set; }

        [MaxLength(2000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Diagnóstico")]
        public string? Diagnosis { get; set; }

        [MaxLength(2000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Trabajo realizado")]
        public string? WorkPerformed { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Estado")]
        public OrderStatus Status { get; set; }

        [MaxLength(1000, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Observación")]
        public string? Observation { get; set; }
    }
}
