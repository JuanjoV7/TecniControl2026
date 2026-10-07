using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Equipo;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IEquiposService
    {
        public Task<Response<CreateEquipoDTO>> CreateAsync(CreateEquipoDTO dto);
        public Task<Response<EquipoDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<EquipoDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<List<EquipoDTO>>> GetByClienteAsync(Guid clienteId);
        public Task<Response<EquipoDTO>> UpdateAsync(UpdateEquipoDTO dto);
    }
}
