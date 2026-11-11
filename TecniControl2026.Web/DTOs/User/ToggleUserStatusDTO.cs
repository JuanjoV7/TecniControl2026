using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.User
{
    public class ToggleUserStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid UserId { get; set; }

        public bool IsActive { get; set; } = false;
    }
}
