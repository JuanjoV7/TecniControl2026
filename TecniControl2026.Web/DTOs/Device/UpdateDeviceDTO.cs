using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Device
{
    public class UpdateDeviceDTO : CreateDeviceDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }
    }
}
