using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.Core
{
    // Order flow: Received → InDiagnosis → InRepair → ReadyForDelivery → Delivered.
    // It can be cancelled at any time before delivery.
    public static class OrderStatusRules
    {
        private static readonly Dictionary<OrderStatus, OrderStatus[]> _transitions = new()
        {
            [OrderStatus.Received] = new[] { OrderStatus.InDiagnosis, OrderStatus.Cancelled },
            [OrderStatus.InDiagnosis] = new[] { OrderStatus.InRepair, OrderStatus.ReadyForDelivery, OrderStatus.Cancelled },
            [OrderStatus.InRepair] = new[] { OrderStatus.InDiagnosis, OrderStatus.ReadyForDelivery, OrderStatus.Cancelled },
            [OrderStatus.ReadyForDelivery] = new[] { OrderStatus.InRepair, OrderStatus.Delivered, OrderStatus.Cancelled },
            [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
        };

        // Statuses the technician can assign when registering the attention
        public static readonly OrderStatus[] AttentionStatuses =
        {
            OrderStatus.InDiagnosis,
            OrderStatus.InRepair,
            OrderStatus.ReadyForDelivery,
        };

        public static bool IsFinal(OrderStatus status)
        {
            return status == OrderStatus.Delivered || status == OrderStatus.Cancelled;
        }

        public static bool CanChange(OrderStatus current, OrderStatus newStatus)
        {
            return _transitions[current].Contains(newStatus);
        }

        public static IEnumerable<OrderStatus> NextStatuses(OrderStatus current)
        {
            return _transitions[current];
        }

        public static string Name(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Received => "Recibida",
                OrderStatus.InDiagnosis => "En diagnóstico",
                OrderStatus.InRepair => "En reparación",
                OrderStatus.ReadyForDelivery => "Lista para entrega",
                OrderStatus.Delivered => "Entregada",
                OrderStatus.Cancelled => "Cancelada",
                _ => status.ToString()
            };
        }
    }
}
