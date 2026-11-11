using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.Device
{
    public class CreateDeviceDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Tipo")]
        public DeviceType Type { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Marca")]
        public string Brand { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Modelo")]
        public string DeviceModel { get; set; } = null!;

        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Serial")]
        public string? SerialNumber { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Cliente")]
        public Guid CustomerId { get; set; }
    }
}
