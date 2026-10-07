using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Usuario
{
    public class CreateUsuarioDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(30, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Documento")]
        public string Documento { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = null!;

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
        public Guid RolId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
