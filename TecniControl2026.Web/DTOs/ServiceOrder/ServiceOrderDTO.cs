using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.ServiceOrder
{
    // Row of the orders list
    public class ServiceOrderDTO
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public DateTime ReceptionDate { get; set; }
        public OrderStatus Status { get; set; }
        public string ReportedIssue { get; set; } = null!;

        public Guid DeviceId { get; set; }
        public string DeviceDescription { get; set; } = null!;

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;

        public Guid? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
    }
}
