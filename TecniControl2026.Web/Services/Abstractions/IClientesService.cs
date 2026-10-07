using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Cliente;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IClientesService
    {
        public Task<Response<CreateClienteDTO>> CreateAsync(CreateClienteDTO dto);
        public Task<Response<ClienteDTO>> GetOneAsync(Guid id);
        public Task<Response<List<ClienteDTO>>> GetActiveAsync();
        public Task<Response<PaginationResponse<ClienteDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<ClienteDTO>> UpdateAsync(UpdateClienteDTO dto);
        public Task<Response<object>> ToggleAsync(ToggleClienteStatusDTO dto);
    }
}
