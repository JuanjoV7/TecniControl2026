using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Customer
{
    public class CreateCustomerDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(30, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Documento")]
        public string Document { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = null!;

        [MaxLength(30, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [EmailAddress(ErrorMessage = "El campo {0} no es un correo válido.")]
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }
    }
}
