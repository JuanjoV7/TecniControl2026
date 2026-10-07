using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Account
{
    public class PerfilDTO
    {
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }

        [Display(Name = "Documento")]
        public string? Documento { get; set; }

        [Display(Name = "Rol")]
        public string? RolNombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = null!;

        [MaxLength(30, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Teléfono")]
        public string? PhoneNumber { get; set; }
    }
}
