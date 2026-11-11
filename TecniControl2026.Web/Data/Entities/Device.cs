using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.Data.Entities
{
    public class Device : IId
    {
        [Key]
        public Guid Id { get; set; }

        public DeviceType Type { get; set; }

        [MaxLength(50)]
        public required string Brand { get; set; }

        [MaxLength(100)]
        public required string DeviceModel { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
    }
}
