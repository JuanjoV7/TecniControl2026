using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Equipo
{
    public class UpdateEquipoDTO : CreateEquipoDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }
    }
}
