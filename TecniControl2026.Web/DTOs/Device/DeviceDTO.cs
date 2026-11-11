using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.Device
{
    public class DeviceDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Tipo")]
        public DeviceType Type { get; set; }

        public string Brand { get; set; } = null!;
        public string DeviceModel { get; set; } = null!;
        public string? SerialNumber { get; set; }

        public Guid CustomerId { get; set; }

        [Display(Name = "Cliente")]
        public string CustomerName { get; set; } = null!;

        public string CustomerDocument { get; set; } = null!;

        public string Description => $"{Type} {Brand} {DeviceModel}" + (string.IsNullOrWhiteSpace(SerialNumber) ? "" : $" (S/N {SerialNumber})");
    }
}
