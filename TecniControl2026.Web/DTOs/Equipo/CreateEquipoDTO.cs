using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.Equipo
{
    public class CreateEquipoDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Tipo")]
        public TipoEquipo Tipo { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Marca")]
        public string Marca { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = null!;

        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Serial")]
        public string? Serial { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Cliente")]
        public Guid ClienteId { get; set; }
    }
}
