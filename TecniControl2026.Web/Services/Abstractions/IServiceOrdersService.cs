using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.ServiceOrder;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IServiceOrdersService
    {
        public Task<Response<ServiceOrderDTO>> CreateAsync(CreateServiceOrderDTO dto);
        public Task<Response<ServiceOrderDetailDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<ServiceOrderDTO>>> GetPaginationAsync(OrdersPaginationRequest request);
        public Task<Response<List<ServiceOrderDTO>>> GetDeviceHistoryAsync(Guid deviceId);
        public Task<Response<object>> AssignTechnicianAsync(AssignTechnicianDTO dto);
        public Task<Response<object>> RegisterAttentionAsync(RegisterAttentionDTO dto);
        public Task<Response<object>> DeliverAsync(DeliverOrderDTO dto);
        public Task<Response<object>> CancelAsync(CancelOrderDTO dto);
    }
}
