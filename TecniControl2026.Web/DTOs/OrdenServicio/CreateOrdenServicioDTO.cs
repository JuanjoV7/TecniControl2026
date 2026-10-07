using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.OrdenServicio
{
    public class CreateOrdenServicioDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Equipo")]
        public Guid EquipoId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(500, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Falla reportada")]
        public string FallaReportada { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Accesorios recibidos")]
        public string? Accesorios { get; set; }

        // Opcional: el técnico se puede asignar al recibir o después
        [Display(Name = "Técnico")]
        public Guid? TecnicoId { get; set; }
    }
}
