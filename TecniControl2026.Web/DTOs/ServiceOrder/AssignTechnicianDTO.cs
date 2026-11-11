using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.ServiceOrder
{
    public class AssignTechnicianDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid ServiceOrderId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Técnico")]
        public Guid TechnicianId { get; set; }
    }
}
