using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.ServiceOrder
{
    public class CreateServiceOrderDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Equipo")]
        public Guid DeviceId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(500, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Falla reportada")]
        public string ReportedIssue { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Accesorios recibidos")]
        public string? Accessories { get; set; }

        // Optional: the technician can be assigned on reception or later
        [Display(Name = "Técnico")]
        public Guid? TechnicianId { get; set; }
    }
}
