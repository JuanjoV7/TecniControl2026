using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.ServiceOrder
{
    public class OrdersPaginationRequest : PaginationRequest
    {
        public OrderStatus? Status { get; set; }
    }
}
