using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Account
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [EmailAddress(ErrorMessage = "El campo {0} debe ser un Email válido")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [MinLength(4, ErrorMessage = "El campo {0} debe tener por lo menos {1} caracteres")]
        [Display(Name = "Contraseña")]
        public required string Password { get; set; }
    }
}