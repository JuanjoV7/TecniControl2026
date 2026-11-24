using System.ComponentModel.DataAnnotations;

namespace TecniControl2026.Web.DTOs.Device
{
    public class ToggleDeviceStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid DeviceId { get; set; }

        public bool IsActive { get; set; }
    }
}
