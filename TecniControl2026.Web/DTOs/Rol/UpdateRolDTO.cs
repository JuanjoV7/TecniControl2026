using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Rol
{
    public class UpdateRolDTO : CreateRolDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }
    }
}
