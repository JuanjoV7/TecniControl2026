using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.OrdenServicio
{
    public class AsignarTecnicoDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid OrdenServicioId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Técnico")]
        public Guid TecnicoId { get; set; }
    }
}
