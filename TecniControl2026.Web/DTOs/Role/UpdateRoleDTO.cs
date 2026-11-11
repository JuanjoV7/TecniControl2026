using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Role
{
    public class UpdateRoleDTO : CreateRoleDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }
    }
}
