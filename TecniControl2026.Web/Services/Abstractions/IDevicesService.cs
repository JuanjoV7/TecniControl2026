using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Device;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IDevicesService
    {
        public Task<Response<CreateDeviceDTO>> CreateAsync(CreateDeviceDTO dto);
        public Task<Response<object>> DeleteAsync(Guid id);
        public Task<Response<DeviceDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<DeviceDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<List<DeviceDTO>>> GetByCustomerAsync(Guid customerId);
        public Task<Response<DeviceDTO>> UpdateAsync(UpdateDeviceDTO dto);
    }
}
