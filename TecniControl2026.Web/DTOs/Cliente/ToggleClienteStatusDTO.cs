using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Cliente
{
    public class ToggleClienteStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid ClienteId { get; set; }

        public bool Activo { get; set; } = false;
    }
}
