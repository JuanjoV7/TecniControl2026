using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.DTOs.OrdenServicio
{
    public class OrdenesPaginationRequest : PaginationRequest
    {
        public EstadoOrden? Estado { get; set; }
    }
}
