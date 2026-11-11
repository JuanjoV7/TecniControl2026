using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Customer;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface ICustomersService
    {
        public Task<Response<CreateCustomerDTO>> CreateAsync(CreateCustomerDTO dto);
        public Task<Response<CustomerDTO>> GetOneAsync(Guid id);
        public Task<Response<List<CustomerDTO>>> GetActiveAsync();
        public Task<Response<PaginationResponse<CustomerDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<CustomerDTO>> UpdateAsync(UpdateCustomerDTO dto);
        public Task<Response<object>> ToggleAsync(ToggleCustomerStatusDTO dto);
    }
}
