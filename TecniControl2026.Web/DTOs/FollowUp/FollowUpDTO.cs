using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.FollowUp
{
    public class FollowUpDTO
    {
        public Guid Id { get; set; }
        public Guid ServiceOrderId { get; set; }
        public DateTime Date { get; set; }
        public string Observation { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public OrderStatus? PreviousStatus { get; set; }
        public OrderStatus? NewStatus { get; set; }
    }
}
