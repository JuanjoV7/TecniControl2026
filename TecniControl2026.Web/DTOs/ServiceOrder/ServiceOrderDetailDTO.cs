using TecniControl2026.Web.DTOs.FollowUp;

namespace TecniControl2026.Web.DTOs.ServiceOrder
{
    public class ServiceOrderDetailDTO : ServiceOrderDTO
    {
        public string? CustomerPhone { get; set; }
        public string ReceivedByName { get; set; } = null!;
        public string? Accessories { get; set; }
        public string? Diagnosis { get; set; }
        public string? WorkPerformed { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }

        public List<FollowUpDTO> FollowUps { get; set; } = new();
    }
}
