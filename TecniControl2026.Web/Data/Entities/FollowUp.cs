using System.ComponentModel.DataAnnotations;
using TecniControl2026.Web.Data.Abstractions;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.Data.Entities
{
    public class FollowUp : IId
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ServiceOrderId { get; set; }
        public ServiceOrder ServiceOrder { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime Date { get; set; }

        [MaxLength(1000)]
        public required string Observation { get; set; }

        // Filled in when the action changed the order status
        public OrderStatus? PreviousStatus { get; set; }
        public OrderStatus? NewStatus { get; set; }
    }
}
