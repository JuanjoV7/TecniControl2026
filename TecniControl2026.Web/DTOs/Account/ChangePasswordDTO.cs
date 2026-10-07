using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Account
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar nueva contraseña")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
