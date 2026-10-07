using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Rol
{
    public class CreateRolDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = null!;

        [MaxLength(200, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Permisos")]
        public List<Guid> PermisoIds { get; set; } = new();
    }
}
