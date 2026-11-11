using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.User
{
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(30, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Documento")]
        public string Document { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Nombres")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(256, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [EmailAddress(ErrorMessage = "El campo {0} no es un correo válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = null!;

        [MaxLength(30, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Teléfono")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Rol")]
        public Guid RoleId { get; set; }
    }
}
