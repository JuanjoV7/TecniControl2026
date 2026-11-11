using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Role
{
    public class CreateRoleDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = null!;

        [MaxLength(200, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "Permisos")]
        public List<Guid> PermissionIds { get; set; } = new();
    }
}
