using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Usuario
{
    public class ToggleUsuarioStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid UsuarioId { get; set; }

        public bool Activo { get; set; } = false;
    }
}
